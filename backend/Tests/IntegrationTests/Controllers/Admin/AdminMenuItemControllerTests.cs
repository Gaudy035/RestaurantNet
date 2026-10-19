using System.Net;
using System.Net.Http.Json;
using backend.DTOs.Auth;
using backend.DTOs.MenuItems;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

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

    private async Task<Category> SeedCategory(string name = "TestCategory") =>
        await DatabaseHelper.ExecuteAsync(_factory, context => MenuSeeder.SeedCategory(context, name));

    [Fact]
    public async Task CreateMenuItem_WithAdminEmployeeUserLoggedInAndValidCategoryId_CreatesItem()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var category = await SeedCategory();

        var dto = new MenuItemCreateDto
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

        var item = await result.ReadJsonAsync<MenuItemResponseDto>();
        Assert.True(item.ItemId > 0);
        Assert.Equal(category.CategoryId, item.CategoryId);
        Assert.Equal("Test", item.Name);
        Assert.Equal("TestUrl", item.ImageUrl);
        Assert.Equal(25, item.Price);
        Assert.True(item.IsAvailable);
        Assert.False(item.IsPinned);

        var persisted = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => context.MenuItems.AsNoTracking().FirstOrDefaultAsync(mi => mi.ItemId == item.ItemId)
        );
        Assert.NotNull(persisted);
        Assert.Equal("Test", persisted.Name);
    }

    [Fact]
    public async Task CreateMenuItem_WithAdminEmployeeUserLoggedInAndInvalidCategoryId_ReturnsNotFound()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var dto = new MenuItemCreateDto
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

        var category = await SeedCategory();

        var dto = new MenuItemCreateDto
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

        var category = await SeedCategory();

        var dto = new MenuItemCreateDto
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

        var category = await SeedCategory();

        var dto = new MenuItemCreateDto
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
    public async Task GetMenuItems_WithAdminEmployeeUserLoggedIn_ReturnsAllMenuItems()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var category = await SeedCategory();
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, name: "Available", isAvailable: true)
        );
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, name: "Unavailable", isAvailable: false)
        );

        var result = await client.GetAsync("/admin/menuitems");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var items = await result.ReadJsonAsync<List<MenuItemResponseDto>>();
        Assert.Equal(2, items.Count);
        Assert.Contains(items, i => i.Name == "Unavailable" && !i.IsAvailable);
        Assert.Contains(items, i => i.Name == "Available" && i.IsAvailable);
    }

    [Fact]
    public async Task GetMenuItems_WithNonAdminEmployeeUserLoggedIn_ReturnsAvailableItemsOnly()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var category = await SeedCategory();
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, name: "Available", isAvailable: true)
        );
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, name: "Unavailable", isAvailable: false)
        );

        var result = await client.GetAsync("/admin/menuitems");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var items = await result.ReadJsonAsync<List<MenuItemResponseDto>>();
        Assert.Single(items);
        Assert.Equal("Available", items.Single().Name);
    }

    [Fact]
    public async Task GetMenuItems_WithSearchParam_ReturnsMatchingItems()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var category = await SeedCategory();
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, name: "ShouldBeFound", isAvailable: true)
        );
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, name: "ShouldAlsoBeFound", isAvailable: false)
        );
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, name: "ShouldNot", isAvailable: true)
        );

        var result = await client.GetAsync("/admin/menuitems?param=befo");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var items = await result.ReadJsonAsync<List<MenuItemResponseDto>>();
        Assert.Equal(2, items.Count);
        Assert.Contains(items, i => i.Name == "ShouldBeFound");
        Assert.Contains(items, i => i.Name == "ShouldAlsoBeFound");
        Assert.DoesNotContain(items, i => i.Name == "ShouldNot");
    }

    [Fact]
    public async Task GetMenuItems_WithNonAdminEmployeeAndSearchParam_ReturnsMatchingAvailableItems()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var category = await SeedCategory();
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, name: "ShouldBeFound", isAvailable: true)
        );
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, name: "ShouldAlsoBeFound", isAvailable: false)
        );

        var result = await client.GetAsync("/admin/menuitems?param=befo");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var items = await result.ReadJsonAsync<List<MenuItemResponseDto>>();
        Assert.Single(items);
        Assert.Equal("ShouldBeFound", items.Single().Name);
    }

    [Fact]
    public async Task GetMenuItems_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var category = await SeedCategory();
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

        var category = await SeedCategory();
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var result = await client.GetAsync("/admin/menuitems");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task GetMenuItemById_WithAdminEmployeeUserLoggedInAndValidId_ReturnsItem()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var category = await SeedCategory();
        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId, name: "Test", imageUrl: "TestUrl", price: 30)
        );

        var result = await client.GetAsync($"/admin/menuitems/{menuItem.ItemId}");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var body = await result.ReadJsonAsync<MenuItemResponseDto>();
        Assert.Equal(menuItem.ItemId, body.ItemId);
        Assert.Equal(category.CategoryId, body.CategoryId);
        Assert.Equal("Test", body.Name);
        Assert.Equal("TestUrl", body.ImageUrl);
        Assert.Equal(30, body.Price);
    }

    [Fact]
    public async Task GetMenuItemById_WithNonAdminEmployeeUserLoggedInAndValidId_ReturnsOk()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var category = await SeedCategory();
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

        var category = await SeedCategory();
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

        var category = await SeedCategory();
        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var result = await client.GetAsync($"/admin/menuitems/{menuItem.ItemId}");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task UpdateMenuItem_WithAdminEmployeeUserLoggedInAndValidData_UpdatesOnlyProvidedFields()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var category1 = await SeedCategory("Category1");
        var category2 = await SeedCategory("Category2");
        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category1.CategoryId, name: "Test", imageUrl: "TestUrl", price: 25)
        );

        var dto = new MenuItemUpdateDto
        {
            CategoryId = category2.CategoryId,
            Price = 42
        };

        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{menuItem.ItemId}", dto);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var body = await result.ReadJsonAsync<MenuItemResponseDto>();
        Assert.Equal(category2.CategoryId, body.CategoryId);
        Assert.Equal(42, body.Price);
        Assert.Equal("Test", body.Name);
        Assert.Equal("TestUrl", body.ImageUrl);

        var persisted = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => context.MenuItems.AsNoTracking().FirstAsync(mi => mi.ItemId == menuItem.ItemId)
        );
        Assert.Equal(category2.CategoryId, persisted.CategoryId);
        Assert.Equal(42, persisted.Price);
        Assert.Equal("Test", persisted.Name);
    }

    [Fact]
    public async Task UpdateMenuItem_WithAdminEmployeeUserLoggedInAndInvalidId_ReturnsNotFound()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var category = await SeedCategory("Category");

        var dto = new MenuItemUpdateDto
        {
            CategoryId = category.CategoryId,
            Price = 42
        };

        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{int.MaxValue}", dto);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task UpdateMenuItem_WithAdminEmployeeUserLoggedInAndInvalidCategoryId_ReturnsNotFound()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var category = await SeedCategory("Category");
        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var dto = new MenuItemUpdateDto { CategoryId = int.MaxValue };

        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{menuItem.ItemId}", dto);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task UpdateMenuItem_WithAdminEmployeeUserLoggedInAndEmptyDto_ReturnsBadRequest()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var category = await SeedCategory("Category");
        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{menuItem.ItemId}", new MenuItemUpdateDto());

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task UpdateMenuItem_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var category = await SeedCategory("Category");
        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{menuItem.ItemId}", new MenuItemUpdateDto { Price = 42 });

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task UpdateMenuItem_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var category = await SeedCategory("Category");
        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{menuItem.ItemId}", new MenuItemUpdateDto { Price = 42 });

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task UpdateMenuItem_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var category = await SeedCategory("Category");
        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context, category.CategoryId)
        );

        var result = await client.PatchAsJsonAsync($"/admin/menuitems/{menuItem.ItemId}", new MenuItemUpdateDto { Price = 42 });

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}
