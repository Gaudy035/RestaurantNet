using System.Net;
using System.Net.Http.Json;
using backend.DTOs.Auth;
using backend.DTOs.MenuItems;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests.Controllers.Admin;

public class AdminMenuItemControllerTests: IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    
    public AdminMenuItemControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(
            new WebApplicationFactoryClientOptions()
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

        var dto = new LoginDto()
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
    public async Task CreateMenuItem_WithAdminEmployeeUserLoggedInAndValidCategoryId_ReturnsOk()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);
        
        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );
        
        var dto = new MenuItemCreateDto()
        {
            CategoryId = category.CategoryId,
            Name = "Test",
            ImageUrl = "TestUrl",
            IsAvailable = true,
            IsPinned = false,
            Price = 25
        };
        
        var result = await client.PostAsJsonAsync("/admin/menuitems", dto);
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateMenuItem_WithAdminEmployeeUserLoggedInAndInvalidCategoryId_ReturnsNotFound()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);
        
        var dto = new MenuItemCreateDto()
        {
            CategoryId = int.MaxValue,
            Name = "Test",
            ImageUrl = "TestUrl",
            IsAvailable = true,
            IsPinned = false,
            Price = 25
        };
        
        var result = await client.PostAsJsonAsync("/admin/menuitems", dto);
        
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateMenuItem_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await EmployeeLogin(client);
        
        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );
        
        var dto = new MenuItemCreateDto()
        {
            CategoryId = category.CategoryId,
            Name = "Test",
            ImageUrl = "TestUrl",
            IsAvailable = true,
            IsPinned = false,
            Price = 25
        };
        
        var result = await client.PostAsJsonAsync("/admin/menuitems", dto);
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateMenuItem_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);
        
        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );
        
        var dto = new MenuItemCreateDto()
        {
            CategoryId = category.CategoryId,
            Name = "Test",
            ImageUrl = "TestUrl",
            IsAvailable = true,
            IsPinned = false,
            Price = 25
        };
        
        var result = await client.PostAsJsonAsync("/admin/menuitems", dto);
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateMenuItem_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );
        
        var dto = new MenuItemCreateDto()
        {
            CategoryId = category.CategoryId,
            Name = "Test",
            ImageUrl = "TestUrl",
            IsAvailable = true,
            IsPinned = false,
            Price = 25
        };
        
        var result = await client.PostAsJsonAsync("/admin/menuitems", dto);
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}