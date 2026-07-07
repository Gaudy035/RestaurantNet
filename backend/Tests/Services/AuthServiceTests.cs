using System.IdentityModel.Tokens.Jwt;
using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Auth;
using backend.Services;
using backend.Tests.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace backend.Tests.Services;

public class AuthServiceTests: IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();

        var _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "very-long-jwt-security-key-for-testing-auth-service-minimum=thirty-two-charatcters-long"
            })
            .Build();

        _authService = new AuthService(_context, _config);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<Client> SeedClientUser()
    {
        var newClient = new Client
        {
            PhoneNumber = "123 123 123",
            User = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("TestPass1234"),
            }
        };

        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();

        return newClient;
    }

    private async Task<Employee> SeedEmployeeUser(bool isAdmin)
    {
        var newEmployee = new Employee
        {
            IsAdmin = isAdmin,
            User = new User
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("TestPass5678"),
            }
        };

        _context.Employees.Add(newEmployee);
        await _context.SaveChangesAsync();

        return newEmployee;
    }

    [Fact]
    public async Task Login_ClientWithCorrectCredentials_ReturnsTokensAndClientRole()
    {
        await SeedClientUser();

        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "TestPass1234"
        };

        var result = await _authService.Login(loginDto, "Client");

        Assert.NotNull(result);
        Assert.Equal("Client", result.Role);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role");

        Assert.NotNull(roleClaim);
        Assert.Equal("Client", roleClaim.Value);
    }

    [Fact]
    public async Task Login_AdminEmployeeWithCorrectCredentialsAnd_ReturnsTokensAndAdminRole()
    {
        await SeedEmployeeUser(true);

        var loginDto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "TestPass5678"
        };

        var result = await _authService.Login(loginDto, "Admin");

        Assert.NotNull(result);
        Assert.Equal("Admin", result.Role);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role");

        Assert.NotNull(roleClaim);
        Assert.Equal("Admin", roleClaim.Value);
    }

    [Fact]
    public async Task Login_NotAdminEmployeeWithCorrectCredentialsAnd_ReturnsTokensAndEmployeeRole()
    {
        await SeedEmployeeUser(false);

        var loginDto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "TestPass5678"
        };

        var result = await _authService.Login(loginDto, "Admin");

        Assert.NotNull(result);
        Assert.Equal("Employee", result.Role);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role");

        Assert.NotNull(roleClaim);
        Assert.Equal("Employee", roleClaim.Value);
    }

    [Fact]
    public async Task Login_WithIncorrectEmail_ReturnsNull()
    {
        await SeedClientUser();

        var loginDto = new LoginDto
        {
            Email = "wrongemail@example.com",
            Password = "TestPass1234"
        };

        var result = await _authService.Login(loginDto, "Client");

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ReturnsNull()
    {
        await SeedClientUser();

        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "IncorrectPassword"
        };

        var result = await _authService.Login(loginDto, "Client");

        Assert.Null(result);
    }
}