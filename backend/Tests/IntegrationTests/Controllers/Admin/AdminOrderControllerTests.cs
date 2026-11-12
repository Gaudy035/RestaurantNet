using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using backend.DTOs.Auth;
using System.Net.Http.Json;
using backend.Data.Entities;
using System.Net;
using backend.DTOs.Orders;
using Microsoft.EntityFrameworkCore;


namespace IntegrationTests.Controllers.Admin;

public class AdminOrderControllerTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public AdminOrderControllerTests(TestWebApplicationFactory factory)
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

    private async Task<Employee> EmployeeLogin(HttpClient client, bool isAdmin = false)
    {
        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, isAdmin: isAdmin)
        );

        var dto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "password1234"
        };

        await client.PostAsJsonAsync("/admin/auth/login", dto);

        return employee;
    }

    private async Task<Client> ClientLogin(HttpClient client)
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

        await client.PostAsJsonAsync("/auth/login", dto);

        return clientUser;
    }

    private async Task<MenuItem> SeedItem(HttpClient client, string itemName = "testItem")
    {
        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCategory(context)
        );

        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context:context, categoryId:category.CategoryId, name: itemName)
        );

        return menuItem;
    }

    private async Task<CartItem> SeedCartItem(HttpClient client, int userId, int itemId, int quantity)
    {
        var cartItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCartItem(context: context, userId: userId, itemId: itemId, quantity: quantity)
        );
        return cartItem;
    }

    [Fact]
    public async Task AddItemToCart_WithCorrectIdsAndAdminUser_AddsItemToCartAndReturns200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var menuItem = await SeedItem(client);

        var result = await client.PostAsync($"admin/orders/cart/{menuItem.ItemId}", null);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var resultCartItem = await result.ReadJsonAsync<CartItemResponseDto>();

        Assert.NotNull(resultCartItem);
        Assert.Equal(resultCartItem.ItemId, menuItem.ItemId);
        Assert.Equal(resultCartItem.Name, menuItem.Name);

        var added = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.CartItems
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.UserId == employeeUser.UserId && ci.ItemId == menuItem.ItemId)
        );

        Assert.NotNull(added);
        Assert.Equal(added.Quantity, resultCartItem.Quantity);
        Assert.Equal(added.ItemId, menuItem.ItemId);
        Assert.Equal(added.UserId, employeeUser.UserId);
    }

    [Fact]
    public async Task AddItemToCart_WithCorrectIdsAndNotAdminEmployeeUser_AddsItemToCartAndReturns200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, false);

        var menuItem = await SeedItem(client);

        var result = await client.PostAsync($"admin/orders/cart/{menuItem.ItemId}", null);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task AddItemToCart_WithCorrectIdsAndClientUser_Returns403()
    {
        var client = CreateClient();

        var clientUser = await ClientLogin(client);

        var menuItem = await SeedItem(client);

        var result = await client.PostAsync($"admin/orders/cart/{menuItem.ItemId}", null);

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task AddItemToCart_WithNonExistingItemId_Returns404()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var result = await client.PostAsync($"admin/orders/cart/{int.MaxValue}", null);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task AddItemToCart_WithItemAlreadyInCart_IncreasesQuantityAndReturns200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var menuItem = await SeedItem(client);
        
        var result1 = await client.PostAsync($"admin/orders/cart/{menuItem.ItemId}", null);

        Assert.Equal(HttpStatusCode.OK, result1.StatusCode);
        
        var result2 = await client.PostAsync($"admin/orders/cart/{menuItem.ItemId}", null);

        Assert.Equal(HttpStatusCode.OK, result2.StatusCode);

        var resultItem2 = await result2.ReadJsonAsync<CartItemResponseDto>();

        Assert.Equal(2, resultItem2.Quantity);
        
        var fetched = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.CartItems
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.UserId == employeeUser.UserId && ci.ItemId == menuItem.ItemId)
        );

        Assert.NotNull(fetched);
        Assert.Equal(2, fetched.Quantity);
        Assert.Equal(menuItem.ItemId, fetched.ItemId);
        Assert.Equal(employeeUser.UserId, fetched.UserId);
    }

    [Fact]
    public async Task RemoveItemFromCart_WithSingleItemAndAdminUser_RemovesItemAndReturns204()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var menuItem = await SeedItem(client);

        var cartItem = await SeedCartItem(client, employeeUser.UserId, menuItem.ItemId, 1);

        var result = await client.DeleteAsync($"admin/orders/cart/{menuItem.ItemId}");

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        var removed = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.CartItems
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.UserId == employeeUser.UserId && ci.ItemId == menuItem.ItemId)
        );

        Assert.Null(removed);
    }

    [Fact]
    public async Task RemoveItemFromCart_WithSingleItemAndNotAdminEmployeeUser_RemovesItemAndReturns204()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, false);

        var menuItem = await SeedItem(client);

        var cartItem = await SeedCartItem(client, employeeUser.UserId, menuItem.ItemId, 1);

        var result = await client.DeleteAsync($"admin/orders/cart/{menuItem.ItemId}");

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task RemoveItem_WithMultipleItemsAndAdminUser_DecreasesQuantityAndReturns200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var menuItem = await SeedItem(client);

        var cartItem = await SeedCartItem(client, employeeUser.UserId, menuItem.ItemId, 2);

        var result = await client.DeleteAsync($"admin/orders/cart/{menuItem.ItemId}");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var resultCartItem = await result.ReadJsonAsync<CartItemResponseDto>();

        Assert.NotNull(resultCartItem);

        Assert.Equal(1, resultCartItem.Quantity);

        var fetched = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.CartItems
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.UserId == employeeUser.UserId && ci.ItemId == menuItem.ItemId)
        );

        Assert.NotNull(fetched);

        Assert.Equal(1, fetched.Quantity);

        Assert.Equal(menuItem.ItemId, fetched.ItemId);

        Assert.Equal(employeeUser.UserId, fetched.UserId);
    }

    [Fact]
    public async Task RemoveItem_WithMultipleItemsAndNonAdminEmployeeUser_DecreasesQuantityAndReturns200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, false);

        var menuItem = await SeedItem(client);

        var cartItem = await SeedCartItem(client, employeeUser.UserId, menuItem.ItemId, 2);

        var result = await client.DeleteAsync($"admin/orders/cart/{menuItem.ItemId}");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task RemoveItemFromCart_WithNonExistingItemId_Returns404()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var result = await client.DeleteAsync($"admin/orders/cart/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task RemoveItemFromCart_ClientUser_Returns403()
    {
        var client = CreateClient();

        var clientUser = await ClientLogin(client);

        var menuItem = await SeedItem(client);

        var cartItem = await SeedCartItem(client, clientUser.UserId, menuItem.ItemId, 1);

        var result = await client.DeleteAsync($"admin/orders/cart/{menuItem.ItemId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
}
