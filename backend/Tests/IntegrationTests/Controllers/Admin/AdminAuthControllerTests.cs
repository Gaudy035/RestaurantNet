using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using backend.DTOs.Auth;
using backend.DTOs.Users;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Controllers.Admin;

public class AdminAuthControllerTests: IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public AdminAuthControllerTests(TestWebApplicationFactory factory)
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
            });
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

    private async Task<bool> RefreshTokenIsActive(string tokenValue) =>
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => context.RefreshTokens.AsNoTracking()
                .Where(rt => rt.TokenValue == tokenValue)
                .Select(rt => rt.IsActive)
                .SingleAsync()
        );

    [Fact]
    public async Task Login_WithValidEmployeeCredentials_ReturnsNoContentAndSetsCookies()
    {
        var client = CreateClient();

        var response = await EmployeeLogin(client);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));

        var setCookies = cookies.ToList();

        Assert.Contains(setCookies, x => x.StartsWith("admin_access_token="));
        Assert.Contains(setCookies, x => x.StartsWith("admin_refresh_token="));

        var accessToken = GetCookieValue(setCookies, "admin_access_token");
        Assert.Equal("Employee", RoleClaim(accessToken));

        var refreshToken = GetCookieValue(setCookies, "admin_refresh_token");
        Assert.True(await RefreshTokenIsActive(refreshToken));
    }

    [Fact]
    public async Task Login_WithAdminCredentials_ReturnsAdminRoleCookie()
    {
        var client = CreateClient();

        var response = await EmployeeLogin(client, isAdmin: true);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));

        var accessToken = GetCookieValue(cookies.ToList(), "admin_access_token");
        Assert.Equal("Admin", RoleClaim(accessToken));
    }

    [Fact]
    public async Task Login_WithInvalidAdminCredentials_ReturnsUnauthorized()
    {
        var client = CreateClient();

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context)
        );

        var dto = new LoginDto
        {
            Email = "worngemail@example.net",
            Password = "wrongpass",
        };

        var response = await client.PostAsJsonAsync("/admin/auth/login", dto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsNoContentAndSetsNewCookies()
    {
        var client = CreateClient();

        var loginResponse = await EmployeeLogin(client);

        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);
        Assert.True(loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies));

        var setCookies = cookies.ToList();
        var oldAccessToken = GetCookieValue(setCookies, "admin_access_token");
        var oldRefreshToken = GetCookieValue(setCookies, "admin_refresh_token");

        var refreshResponse = await client.PostAsync("/admin/auth/refresh", null);

        Assert.Equal(HttpStatusCode.NoContent, refreshResponse.StatusCode);
        Assert.True(refreshResponse.Headers.TryGetValues("Set-Cookie", out var cookies2));

        var setCookies2 = cookies2.ToList();
        var newAccessToken = GetCookieValue(setCookies2, "admin_access_token");
        var newRefreshToken = GetCookieValue(setCookies2, "admin_refresh_token");

        Assert.NotEqual(oldAccessToken, newAccessToken);
        Assert.NotEqual(oldRefreshToken, newRefreshToken);

        // The consumed refresh token must be revoked in the database.
        Assert.False(await RefreshTokenIsActive(oldRefreshToken));
        Assert.True(await RefreshTokenIsActive(newRefreshToken));
    }

    [Fact]
    public async Task Refresh_WithMissingRefreshToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/admin/auth/refresh", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithClientUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var response = await client.PostAsync("/admin/auth/refresh", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithLoggedInUser_ReturnsNoContentAndClearsCookies()
    {
        var client = CreateClient();

        var loginResponse = await EmployeeLogin(client);

        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);
        Assert.True(loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies.ToList(), x => x.StartsWith("admin_refresh_token="));

        var logoutResponse = await client.PostAsync("/admin/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        Assert.True(logoutResponse.Headers.TryGetValues("Set-Cookie", out var cookies2));

        var setCookies2 = cookies2.ToList();
        var accessToken = GetCookieValue(setCookies2, "admin_access_token");
        var clearedRefreshToken = GetCookieValue(setCookies2, "admin_refresh_token");

        Assert.Equal("", accessToken);
        Assert.Equal("", clearedRefreshToken);
    }

    [Fact]
    public async Task Me_WithLoggedInEmployeeUser_ReturnsEmployeeData()
    {
        var client = CreateClient();

        var loginResponse = await EmployeeLogin(client);

        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);

        var meResponse = await client.GetAsync("/admin/auth/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var body = await meResponse.ReadJsonAsync<EmployeeResponseDto>();
        Assert.Equal("Jane", body.FirstName);
        Assert.Equal("Doe", body.LastName);
        Assert.Equal("jane@example.com", body.Email);
        Assert.False(body.IsAdmin);
    }

    [Fact]
    public async Task Me_WithLoggedInClientUser_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var response = await client.GetAsync("/admin/auth/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
