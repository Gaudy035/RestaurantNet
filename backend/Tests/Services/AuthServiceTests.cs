using System.IdentityModel.Tokens.Jwt;
using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Auth;
using backend.Services;
using backend.Tests.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == result.RefreshToken);
        
        Assert.NotNull(refreshToken);
        Assert.Equal("Client", refreshToken.Role);
    }

    [Fact]
    public async Task Login_AdminEmployeeWithCorrectCredentials_ReturnsTokensAndAdminRole()
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

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == result.RefreshToken);
        
        Assert.NotNull(refreshToken);
        Assert.Equal("Admin", refreshToken.Role);
    }

    [Fact]
    public async Task Login_NotAdminEmployeeWithCorrectCredentials_ReturnsTokensAndEmployeeRole()
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

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == result.RefreshToken);

        Assert.NotNull(refreshToken);
        Assert.Equal("Employee", refreshToken.Role);
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

    [Fact]
    public async Task Login_EmployeeTryingToLoginFromClientForm_ReturnsNull()
    {
        await SeedEmployeeUser(false);

        var loginDto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "TestPass5678"
        };

        var result = await _authService.Login(loginDto, "Client");

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_AdminTryingToLoginFromClientForm_ReturnsNull()
    {
        await SeedEmployeeUser(true);

        var loginDto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "TestPass5678"
        };

        var result = await _authService.Login(loginDto, "Client");

        Assert.Null(result);
    }
    
    [Fact]
    public async Task Login_ClientTryingToLoginFromAdminForm_ReturnsNull()
    {
        await SeedClientUser();

        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "TestPass1234"
        };

        var result = await _authService.Login(loginDto, "Admin");

        Assert.Null(result);
    }

    [Fact]
    public async Task RevokeToken_WithCorrectTokenValue_MarksTokenInactiveAndSetsRevokedAt()
    {
        await SeedClientUser();

        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "TestPass1234"
        };

        var result = await _authService.Login(loginDto, "Client");

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == result!.RefreshToken);
        
        Assert.True(token!.IsActive);
        Assert.Null(token!.RevokedAt);
        
        await _authService.RevokeToken(result!.RefreshToken);

        Assert.False(token.IsActive);
        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public async Task Refresh_WithCorrectTokenValue_GeneratesNewCorrectTokens()
    {
        await SeedClientUser();

        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "TestPass1234"
        };

        var loginResponse = await _authService.Login(loginDto, "Client");

        var refreshResponse = await _authService.Refresh(loginResponse!.RefreshToken);

        Assert.NotEqual(loginResponse.AccessToken, refreshResponse!.AccessToken);
        Assert.NotEqual(loginResponse.RefreshToken, refreshResponse!.RefreshToken);
        Assert.Equal(loginResponse.Role, refreshResponse.Role);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(refreshResponse.AccessToken);
        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role");

        Assert.NotNull(roleClaim);
        Assert.Equal("Client", roleClaim.Value);

        var oldRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == loginResponse.RefreshToken);
        
        var newRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == refreshResponse.RefreshToken);

        Assert.Equal(oldRefreshToken!.UserId, newRefreshToken!.UserId);
        Assert.Equal("Client", newRefreshToken.Role);
        Assert.False(oldRefreshToken.IsActive);
    }
}