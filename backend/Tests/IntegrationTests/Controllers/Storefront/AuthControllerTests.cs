using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using backend.DTOs.Auth;
using backend.DTOs.Users;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Controllers.Storefront;

public class AuthControllerTests: IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                HandleCookies = true,
                AllowAutoRedirect = false
            }
        );
    }

    private static string GetCookieValue(IEnumerable<string> cookies, string name)
    {
        var cookie = cookies.Single(x => x.StartsWith($"{name}="));
        var value = cookie.Split(';')[0].Substring(name.Length + 1);
        // ASP.NET Core percent-encodes characters such as '=', '+' and '/' in cookie values.
        return Uri.UnescapeDataString(value.Trim('"'));
    }

    private static string RoleClaim(string accessToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        return jwt.Claims.First(c => c.Type == "role").Value;
    }

    private async Task<HttpResponseMessage> ClientLogin(HttpClient client)
    {
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context)
        );

        var dto = new LoginDto
        {
            Email = "john@example.com",
            Password = "password1234"
        };

        var response = await client.PostAsJsonAsync("auth/login", dto);

        return response;
    }

    private async Task<HttpResponseMessage> EmployeeLogin(HttpClient client, bool isAdmin = false)
    {
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, isAdmin: isAdmin)
        );

        var dto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "password1234",
        };

        var response = await client.PostAsJsonAsync("/admin/auth/login", dto);
        return response;
    }

    private async Task<bool> RefreshTokenIsActive(string tokenValue) =>
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => context.RefreshTokens.AsNoTracking()
                .Where(rt => rt.TokenValue == tokenValue)
                .Select(rt => rt.IsActive)
                .SingleAsync()
        );

    [Fact]
    public async Task Register_WithProperData_CreatesClientAccountAndSetsCookies()
    {
        var client = CreateClient();

        var dto = new ClientCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "password1234",
            PhoneNumber = "123 123 123"
        };

        var response = await client.PostAsJsonAsync("auth/register", dto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));

        var setCookies = cookies.ToList();
        Assert.Contains(setCookies, x => x.StartsWith("client_access_token="));
        Assert.Contains(setCookies, x => x.StartsWith("client_refresh_token="));

        var persisted = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => context.Clients.AsNoTracking().Include(c => c.User)
                .FirstOrDefaultAsync(c => c.User.Email == "john@example.com")
        );

        Assert.NotNull(persisted);
        Assert.Equal("John", persisted!.User.FirstName);
        Assert.Equal("Doe", persisted.User.LastName);
        Assert.Equal("123 123 123", persisted.PhoneNumber);
        Assert.True(BCrypt.Net.BCrypt.Verify("password1234", persisted.User.Password));

        var refreshToken = GetCookieValue(setCookies, "client_refresh_token");
        Assert.True(await RefreshTokenIsActive(refreshToken));
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var client = CreateClient();

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context)
        );

        var dto = new ClientCreateDto
        {
            FirstName = "John",
            LastName = "Smith",
            Email = "john@example.com",
            Password = "password1234",
            PhoneNumber = "123 431 789"
        };

        var response = await client.PostAsJsonAsync("auth/register", dto);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsNoContentAndSetsCookies()
    {
        var client = CreateClient();

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context)
        );

        var dto = new LoginDto
        {
            Email = "john@example.com",
            Password = "password1234"
        };

        var response = await client.PostAsJsonAsync("auth/login", dto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));

        var setCookies = cookies.ToList();

        Assert.Contains(setCookies, x => x.StartsWith("client_access_token="));
        Assert.Contains(setCookies, x => x.StartsWith("client_refresh_token="));

        var accessToken = GetCookieValue(setCookies, "client_access_token");
        Assert.Equal("Client", RoleClaim(accessToken));

        var refreshToken = GetCookieValue(setCookies, "client_refresh_token");
        Assert.True(await RefreshTokenIsActive(refreshToken));
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var client = CreateClient();

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context)
        );

        var dto = new LoginDto
        {
            Email = "invalidemail@example.com",
            Password = "wrongpassword"
        };

        var response = await client.PostAsJsonAsync("auth/login", dto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithLoggedInUser_ReturnsNoContentAndClearsCookies()
    {
        var client = CreateClient();

        var loginResponse = await ClientLogin(client);

        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);
        Assert.True(loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies));

        var logoutResponse = await client.PostAsync("auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        Assert.True(logoutResponse.Headers.TryGetValues("Set-Cookie", out var cookies2));
        var setCookies = cookies2.ToList();

        var accessToken = GetCookieValue(setCookies, "client_access_token");
        var clearedRefreshToken = GetCookieValue(setCookies, "client_refresh_token");

        Assert.Equal("", accessToken);
        Assert.Equal("", clearedRefreshToken);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsNoContentAndSetsNewCookies()
    {
        var client = CreateClient();

        var loginResponse = await ClientLogin(client);

        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);
        Assert.True(loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies));

        var setCookies = cookies.ToList();
        var oldAccessToken = GetCookieValue(setCookies, "client_access_token");
        var oldRefreshToken = GetCookieValue(setCookies, "client_refresh_token");

        var refreshResponse = await client.PostAsync("auth/refresh", null);

        Assert.Equal(HttpStatusCode.NoContent, refreshResponse.StatusCode);
        Assert.True(refreshResponse.Headers.TryGetValues("Set-Cookie", out var cookies2));

        var setCookies2 = cookies2.ToList();
        var newAccessToken = GetCookieValue(setCookies2, "client_access_token");
        var newRefreshToken = GetCookieValue(setCookies2, "client_refresh_token");

        Assert.NotEqual(oldAccessToken, newAccessToken);
        Assert.NotEqual(oldRefreshToken, newRefreshToken);

        Assert.False(await RefreshTokenIsActive(oldRefreshToken));
        Assert.True(await RefreshTokenIsActive(newRefreshToken));
    }

    [Fact]
    public async Task Refresh_WithEmployeeUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var response = await client.PostAsync("auth/refresh", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithMissingRefreshToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsync("auth/refresh", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithLoggedInClientUser_ReturnsClientData()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var response = await client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.ReadJsonAsync<ClientResponseDto>();
        Assert.Equal("John", body.FirstName);
        Assert.Equal("Doe", body.LastName);
        Assert.Equal("john@example.com", body.Email);
    }

    [Fact]
    public async Task Me_WithLoggedInEmployeeUser_ReturnsUnauthorized()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var response = await client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithNoLoggedInUser_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync("auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
