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
    
    [Fact]
    public async Task GetMenuItems_WithAdminEmployeeUserLoggedIn_ReturnsOkAndAllMenuItems()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, isAvailable: true)
        );
        
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, isAvailable: false)
        );

        var result = await client.GetAsync("/admin/menuitems");
        var items = await result.Content.ReadFromJsonAsync<IEnumerable<MenuItemResponseDto>>();
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(2, items!.Count());
    }
    
    [Fact]
    public async Task GetMenuItems_WithNonAdminEmployeeUserLoggedIn_ReturnsOkAndAvailableItems()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client);

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, isAvailable: true)
        );
        
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, isAvailable: false)
        );

        var result = await client.GetAsync("/admin/menuitems");
        var items = await result.Content.ReadFromJsonAsync<IEnumerable<MenuItemResponseDto>>();
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Single(items!);
    }
    
    [Fact]
    public async Task GetMenuItems_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var result = await client.GetAsync("/admin/menuitems");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task GetMenuItems_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var result = await client.GetAsync("/admin/menuitems");
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task GetMenuItemById_WithAdminEmployeeUserLoggedInAndValidId_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );

        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );
        
        var result = await client.GetAsync($"/admin/menuitems/{menuItem.ItemId}");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetMenuItemById_WithNonAdminEmployeeUserLoggedInAndValidId_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client);

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );

        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );
        
        var result = await client.GetAsync($"/admin/menuitems/{menuItem.ItemId}");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetMenuItemById_WithValidUserLoggedInAndInvalidId_ReturnsNotFound()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);
        
        var result = await client.GetAsync($"/admin/menuitems/{int.MinValue}");
        
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
    
    [Fact]
    public async Task GetMenuItemById_WithClientLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );

        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );
        
        var result = await client.GetAsync($"/admin/menuitems/{menuItem.ItemId}");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task GetMenuItemById_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );

        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );
        
        var result = await client.GetAsync($"/admin/menuitems/{menuItem.ItemId}");
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
    
    [Fact]
    public async Task UpdateMenuItem_WithAdminEmployeeUserLoggedInAndValidData_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var category1 = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context, "Category1")
        );
        var category2 = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context, "Category2")
        );

        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category1.CategoryId)
        );

        var dto = new MenuItemUpdateDto
        {
            CategoryId = category1.CategoryId,
            Price = 42
        };
        
        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{menuItem.ItemId}", dto);
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    
    [Fact]
    public async Task UpdateMenuItem_WithAdminEmployeeUserLoggedInAndInvalidId_ReturnsNotFound()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context, "Category")
        );

        var dto = new MenuItemUpdateDto
        {
            CategoryId = category.CategoryId,
            Price = 42
        };
        
        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{int.MaxValue}", dto);
        
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
    
    [Fact]
    public async Task UpdateMenuItem_WithAdminEmployeeUserLoggedInAndEmptyDto_ReturnsBadRequest()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context, "Category")
        );

        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var dto = new MenuItemUpdateDto { };
        
        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{menuItem.ItemId}", dto);
        
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task UpdateMenuItem_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client);

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context, "Category")
        );

        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var dto = new MenuItemUpdateDto
        {
            Price = 42
        };
        
        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{menuItem.ItemId}", dto);
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task UpdateMenuItem_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context, "Category")
        );

        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var dto = new MenuItemUpdateDto
        {
            Price = 42
        };
        
        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{menuItem.ItemId}", dto);
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task UpdateMenuItem_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context, "Category")
        );

        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var dto = new MenuItemUpdateDto
        {
            Price = 42
        };
        
        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{menuItem.ItemId}", dto);
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}