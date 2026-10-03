using System.Net;
using System.Net.Http.Json;
using backend.DTOs.Auth;
using backend.DTOs.Users;
using Docker.DotNet.Handler.Abstractions;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

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
        return cookie.Split(';')[0].Substring(name.Length + 1);
    }

    private async Task<HttpResponseMessage> ClientLogin(HttpClient client)
    {
        var clientUser = await DatabaseHelper.ExecuteAsync(
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

    [Fact]
    public async Task Register_WithProperData_ReturnsNoContentAndSetsCookies()
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
        Assert.True(loginResponse.Headers.Contains("Set-Cookie"));
        
        var logoutResponse = await client.PostAsync("auth/logout", null);
        
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        Assert.True(logoutResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
        var setCookies = cookies.ToList();
        
        var accessToken = GetCookieValue(setCookies, "client_access_token");
        var refreshToken = GetCookieValue(setCookies, "client_refresh_token");
        
        Assert.Equal("", accessToken);
        Assert.Equal("", refreshToken);
    }
    
    [Fact]
    public async Task Refresh_WithLoggedInUser_ReturnsNoContentAndSetsCookies()
    {
        var client = CreateClient();
        
        var loginResponse = await ClientLogin(client);
        
        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);
        Assert.True(loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
        
        var setCookies = cookies.ToList();
        Assert.Contains(setCookies, x => x.StartsWith("client_access_token="));
        Assert.Contains(setCookies, x => x.StartsWith("client_refresh_token="));
        
        var oldAccessToken = GetCookieValue(setCookies, "client_access_token");
        var oldRefreshToken = GetCookieValue(setCookies, "client_refresh_token");
        
        var refreshResponse = await client.PostAsync("auth/refresh", null);
        
        Assert.Equal(HttpStatusCode.NoContent, refreshResponse.StatusCode);
        Assert.True(refreshResponse.Headers.TryGetValues("Set-Cookie", out var cookies2));
        
        var setCookies2 = cookies2.ToList();
        Assert.Contains(setCookies2, x => x.StartsWith("client_access_token="));
        Assert.Contains(setCookies2, x => x.StartsWith("client_refresh_token="));
        
        var newAccessToken = GetCookieValue(setCookies2, "client_access_token");
        var newRefreshToken = GetCookieValue(setCookies2, "client_refresh_token");
        
        Assert.NotEqual(oldAccessToken, newAccessToken);
        Assert.NotEqual(oldRefreshToken, newRefreshToken);
    }
    
    [Fact]
    public async Task Refresh_WithMissingRefreshToken_ReturnsUnauthorized()
    {
        var client = CreateClient();
        
        var response = await client.PostAsync("auth/refresh", null);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task Me_WithLoggedInUser_ReturnsOk()
    {
        var client = CreateClient();

        var loginResponse = await ClientLogin(client);

        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);

        var meResponse = await client.GetAsync("/auth/me");
         
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
    }
}
