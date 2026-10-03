using System.Net;
using System.Net.Http.Json;
using backend.DTOs.Auth;
using backend.DTOs.Locations;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests.Controllers.Admin;

public class AdminLocationControllerTests:IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public AdminLocationControllerTests(TestWebApplicationFactory factory)
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
    
    private async Task EmployeeLogin(HttpClient client, bool isAdmin = false)
    {
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, isAdmin: isAdmin)
        );

        var dto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "password1234"
        };
        
        await client.PostAsJsonAsync("/admin/auth/login", dto);
    }
    
    private async Task ClientLogin(HttpClient client)
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
        
        await client.PostAsJsonAsync("auth/login", dto);
    }

    [Fact]
    public async Task CreateLocation_WithAdminEmployeeUserLoggedIn_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var dto = new LocationCreateDto
        {
            City = "TestCity",
            Address = "Test Address 42"
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/", dto);
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    [Fact]
    public async Task CreateLocation_WithAdminEmployeeUserLoggedInAndDuplicateData_ReturnsConflict()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var dto = new LocationCreateDto
        {
            City = "TestCity",
            Address = "Test Address 42"
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/", dto);
        
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateLocation_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client);

        var dto = new LocationCreateDto
        {
            Address = "Test Address",
            City = "Test City",
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/", dto);
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateLocation_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);

        var dto = new LocationCreateDto
        {
            Address = "Test Address",
            City = "Test City",
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/", dto);
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateLocation_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var dto = new LocationCreateDto
        {
            Address = "Test Address",
            City = "Test City",
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/", dto);
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}