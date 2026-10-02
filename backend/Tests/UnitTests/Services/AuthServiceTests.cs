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

    private async Task<RefreshToken> SeedRefreshToken(DateTimeOffset expiration, bool isActive = true)
    {
        var user = await SeedClientUser();

        var newRefreshToken = new RefreshToken
        {
            TokenValue = "testRefreshTokenValue",
            ExpiresAt = expiration,
            IsActive = isActive,
            Role = "Client",
            UserId = user.UserId
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return newRefreshToken;
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

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Client", result.Data.Role);
        Assert.False(string.IsNullOrEmpty(result.Data.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.Data.RefreshToken));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Data.AccessToken);
        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role");

        Assert.NotNull(roleClaim);
        Assert.Equal("Client", roleClaim.Value);

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == result.Data.RefreshToken);
        
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

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Admin", result.Data.Role);
        Assert.False(string.IsNullOrEmpty(result.Data.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.Data.RefreshToken));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Data.AccessToken);
        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role");

        Assert.NotNull(roleClaim);
        Assert.Equal("Admin", roleClaim.Value);

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == result.Data.RefreshToken);
        
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

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Employee", result.Data.Role);
        Assert.False(string.IsNullOrEmpty(result.Data.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.Data.RefreshToken));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Data.AccessToken);
        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role");

        Assert.NotNull(roleClaim);
        Assert.Equal("Employee", roleClaim.Value);

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == result.Data.RefreshToken);

        Assert.NotNull(refreshToken);
        Assert.Equal("Employee", refreshToken.Role);
    }

    [Fact]
    public async Task Login_WithIncorrectEmail_ReturnsInvalidCredentialsError()
    {
        await SeedClientUser();

        var loginDto = new LoginDto
        {
            Email = "wrongemail@example.com",
            Password = "TestPass1234"
        };

        var result = await _authService.Login(loginDto, "Client");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(401, result.Error.StatusCode);
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ReturnsInvalidCredentialsError()
    {
        await SeedClientUser();

        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "IncorrectPassword"
        };

        var result = await _authService.Login(loginDto, "Client");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(401, result.Error.StatusCode);
    }

    [Fact]
    public async Task Login_EmployeeTryingToLoginFromClientForm_ReturnsInvalidCredentialsError()
    {
        await SeedEmployeeUser(false);

        var loginDto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "TestPass5678"
        };

        var result = await _authService.Login(loginDto, "Client");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(401, result.Error.StatusCode);
    }

    [Fact]
    public async Task Login_AdminTryingToLoginFromClientForm_ReturnsInvalidCredentialsError()
    {
        await SeedEmployeeUser(true);

        var loginDto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "TestPass5678"
        };

        var result = await _authService.Login(loginDto, "Client");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(401, result.Error.StatusCode);
    }
    
    [Fact]
    public async Task Login_ClientTryingToLoginFromAdminForm_ReturnsInvalidCredentialsError()
    {
        await SeedClientUser();

        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "TestPass1234"
        };

        var result = await _authService.Login(loginDto, "Admin");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(401, result.Error.StatusCode);
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
            .FirstOrDefaultAsync(rt => rt.TokenValue == result.Data!.RefreshToken);
        
        Assert.True(token!.IsActive);
        Assert.Null(token!.RevokedAt);
        
        await _authService.RevokeToken(result.Data!.RefreshToken);

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

        var refreshResponse = await _authService.Refresh(loginResponse.Data!.RefreshToken);

        Assert.NotEqual(loginResponse.Data.AccessToken, refreshResponse.Data!.AccessToken);
        Assert.NotEqual(loginResponse.Data.RefreshToken, refreshResponse.Data!.RefreshToken);
        Assert.Equal(loginResponse.Data.Role, refreshResponse.Data.Role);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(refreshResponse.Data.AccessToken);
        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role");

        Assert.NotNull(roleClaim);
        Assert.Equal("Client", roleClaim.Value);

        var oldRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == loginResponse.Data.RefreshToken);
        
        var newRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == refreshResponse.Data.RefreshToken);

        Assert.Equal(oldRefreshToken!.UserId, newRefreshToken!.UserId);
        Assert.Equal("Client", newRefreshToken.Role);
        Assert.False(oldRefreshToken.IsActive);
    }

    [Fact]
    public async Task Refresh_WithInactiveRefreshToken_ReturnsInvalidRefreshTokenError()
    {
        var testRefreshToken = await SeedRefreshToken(DateTimeOffset.UtcNow.AddDays(1), false);

        var result = await _authService.Refresh(testRefreshToken.TokenValue);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(401, result.Error.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_ReturnsInvalidRefreshTokenErrorAndSetsRevoked()
    {
        var testRefreshToken = await SeedRefreshToken(DateTimeOffset.UtcNow.AddDays(-1), true);

        var result = await _authService.Refresh(testRefreshToken.TokenValue);

        var testRefreshTokenRefetch = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == testRefreshToken.TokenValue);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(401, result.Error.StatusCode);
        Assert.False(testRefreshTokenRefetch!.IsActive);
        Assert.NotNull(testRefreshTokenRefetch!.RevokedAt);
    }

    [Fact]
    public async Task Refresh_WithIncorrectRefreshTokenValue_ReturnsInvalidRefreshTokenError()
    {
        await SeedRefreshToken(DateTimeOffset.UtcNow.AddDays(7), true);

        var result = await _authService.Refresh("IncorrectTokenValue");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(401, result.Error.StatusCode);
    }

    [Fact]
    public async Task MeClient_WithProperUserId_ReturnsCorrectData()
    {
        var client = await SeedClientUser();

        var result = await _authService.MeClient(client.UserId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(client.UserId, result.Data.UserId);
        Assert.Equal("John", result.Data.FirstName);
        Assert.Equal("Doe", result.Data.LastName);
        Assert.Equal("john@example.com", result.Data.Email);
        Assert.Equal("123 123 123", result.Data.PhoneNumber);
    }

    [Fact]
    public async Task MeClient_WithEmployeeUser_ReturnsClientNotFoundError()
    {
        var employee = await SeedEmployeeUser(false);

        var result = await _authService.MeClient(employee.UserId);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
    }

    [Fact]
    public async Task MeClient_WithIncorrectId_ReturnsClientNotFoundError()
    {
        // Random number not corresponding to any user in db
        var result = await _authService.MeClient(42);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
    }

    [Fact]
    public async Task MeAdmin_WithCorrectIdOfAdmin_ReturnsCorrectData()
    {
        var admin = await SeedEmployeeUser(true);

        var result = await _authService.MeAdmin(admin.UserId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(admin.UserId, result.Data.UserId);
        Assert.Equal("Jane", result.Data.FirstName);
        Assert.Equal("Doe", result.Data.LastName);
        Assert.Equal("jane@example.com", result.Data.Email);
        Assert.True(result.Data.IsAdmin);
    }
    
    [Fact]
    public async Task MeAdmin_WithCorrectIdOfNonAdmin_ReturnsCorrectData()
    {
        var admin = await SeedEmployeeUser(false);

        var result = await _authService.MeAdmin(admin.UserId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(admin.UserId, result.Data.UserId);
        Assert.Equal("Jane", result.Data.FirstName);
        Assert.Equal("Doe", result.Data.LastName);
        Assert.Equal("jane@example.com", result.Data.Email);
        Assert.False(result.Data.IsAdmin);
    }

    [Fact]
    public async Task MeAdmin_WithClientId_ReturnsEmployeeNotFoundError()
    {
        var client = await SeedClientUser();

        var result = await _authService.MeAdmin(client.UserId);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
    }

    [Fact]
    public async Task MeAdmin_WithIncorrectId_ReturnsEmployeeNotFoundError()
    {
        // Random number not corresponding to any user in db
        var result = await _authService.MeAdmin(42);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
    }
}