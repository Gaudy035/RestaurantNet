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

    private async Task<MenuItem> SeedItem(
        string itemName = "testItem", 
        decimal price = 10.00m,
        string? categoryName = null
    ){
        var category = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => categoryName == null
                ? MenuSeeder.SeedCategory(context: context)
                : MenuSeeder.SeedCategory(context: context, categoryName: categoryName)
        );

        var menuItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedMenuItem(context:context, categoryId:category.CategoryId, name: itemName, price: price)
        );

        return menuItem;
    }

    private async Task<CartItem> SeedCartItem(int userId, int itemId, int quantity)
    {
        var cartItem = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => MenuSeeder.SeedCartItem(context: context, userId: userId, itemId: itemId, quantity: quantity)
        );
        return cartItem;
    }

    private async Task<Order> SeedOrder(
        List<MenuItem> items,
        int locationId,
        bool delivery = false
    )
    {
        var newOrder = new Order
        {
            LocationId = locationId,
            OrderTime = DateTime.UtcNow,
            Price = items.Sum(i => i.Price),
            PhoneNumber = "123 123 123",
            PaymentMethod = PaymentMethod.Cash,
            OrderType = delivery ? OrderType.Delivery : OrderType.DineIn,
            IsPaid = false,
            Status = OrderStatus.Preparing,
            OrderItems = items.Select(i => new OrderItem
            {
                ItemId = i.ItemId,
                Price = i.Price,
                Quantity = 1
            }).ToList(),
            Delivery = delivery
                ? new Delivery
                {
                    City = "Test City 1",
                    Address = "Test Address 1"
                }
                : null
        };

        var createdOrder = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => {
                context.Orders.Add(newOrder);
                await context.SaveChangesAsync();
                return newOrder;
            }
        );

        return createdOrder;
    }

    [Fact]
    public async Task AddItemToCart_WithCorrectIdsAndAdminUser_AddsItemToCartAndReturns200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var menuItem = await SeedItem();

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

        var menuItem = await SeedItem();

        var result = await client.PostAsync($"admin/orders/cart/{menuItem.ItemId}", null);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task AddItemToCart_WithCorrectIdsAndClientUser_Returns403()
    {
        var client = CreateClient();

        var clientUser = await ClientLogin(client);

        var menuItem = await SeedItem();

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

        var menuItem = await SeedItem();
        
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

        var menuItem = await SeedItem();

        var cartItem = await SeedCartItem(employeeUser.UserId, menuItem.ItemId, 1);

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

        var menuItem = await SeedItem();

        var cartItem = await SeedCartItem(employeeUser.UserId, menuItem.ItemId, 1);

        var result = await client.DeleteAsync($"admin/orders/cart/{menuItem.ItemId}");

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task RemoveItem_WithMultipleItemsAndAdminUser_DecreasesQuantityAndReturns200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var menuItem = await SeedItem();

        var cartItem = await SeedCartItem(employeeUser.UserId, menuItem.ItemId, 2);

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

        var menuItem = await SeedItem();

        var cartItem = await SeedCartItem(employeeUser.UserId, menuItem.ItemId, 2);

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

        var menuItem = await SeedItem();

        var cartItem = await SeedCartItem(clientUser.UserId, menuItem.ItemId, 1);

        var result = await client.DeleteAsync($"admin/orders/cart/{menuItem.ItemId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetCart_WithItemsInCartAndAdminUser_ReturnsAllItemsAndCorrectTotalPriceAnd200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var menuItem1 = await SeedItem("item1", 15.00m);
        var menuItem2 = await SeedItem("item2", 20.00m, "Category2");

        var cartItem1 = await SeedCartItem(employeeUser.UserId, menuItem1.ItemId, 2);
        var cartItem2 = await SeedCartItem(employeeUser.UserId, menuItem2.ItemId, 3);
        
        var result = await client.GetAsync($"admin/orders/cart");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        
        var resultCart = await result.ReadJsonAsync<CartResponseDto>();
        
        Assert.NotNull(resultCart);
        Assert.Equal(2, resultCart.Items.Count);
        Assert.Equal(cartItem1.Quantity * menuItem1.Price + cartItem2.Quantity * menuItem2.Price, resultCart.TotalPrice);
    }

    [Fact]
    public async Task GetCart_WithNoItemsAndAdminUser_ReturnsEmptyListAndTotalPriceZeroAnd200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var result = await client.GetAsync($"admin/orders/cart");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var resultCart = await result.ReadJsonAsync<CartResponseDto>();

        Assert.NotNull(resultCart);
        Assert.Empty(resultCart.Items);
        Assert.Equal(0, resultCart.TotalPrice);
    }

    [Fact]
    public async Task GetCart_WithNonAdminEmployeeUser_ReturnsCartAnd200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, false);

        var menuItem1 = await SeedItem("item1", 15.00m);

        await SeedCartItem(employeeUser.UserId, menuItem1.ItemId, 2);

        var result = await client.GetAsync($"admin/orders/cart");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetCart_WithClientUser_Returns403()
    {
        var client = CreateClient();

        var clientUser = await ClientLogin(client);

        var result = await client.GetAsync($"admin/orders/cart");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task ClearCart_WithAdminUser_ClearsCartAndReturns204()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var item1 = await SeedItem("item1", 15.00m);
        var item2 = await SeedItem("item2", 10.00m, "category2");

        await SeedCartItem(employeeUser.UserId, item1.ItemId, 1);
        await SeedCartItem(employeeUser.UserId, item2.ItemId, 3);

        var cartCountBefore = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.CartItems
                .AsNoTracking()
                .Where(ci => ci.UserId == employeeUser.UserId)
                .CountAsync()
        );

        Assert.Equal(2, cartCountBefore);

        var result = await client.DeleteAsync("/admin/orders/cart");

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        var cartCountAfter = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.CartItems
                .AsNoTracking()
                .Where(ci => ci.UserId == employeeUser.UserId)
                .CountAsync()
        );

        Assert.Equal(0, cartCountAfter);
    }

    [Fact]
    public async Task ClearCart_WithNonAdminEmployeeUser_ClearsCartAndReturns204()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, false);

        var item = await SeedItem();

        await SeedCartItem(employeeUser.UserId, item.ItemId, 2);

        var result = await client.DeleteAsync("/admin/orders/cart");

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        var countAfter = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.CartItems
                .AsNoTracking()
                .Where(ci => ci.UserId == employeeUser.UserId)
                .CountAsync()
        );

        Assert.Equal(0, countAfter);
    }

    [Fact]
    public async Task ClearCart_WithClientUser_Returns403()
    {
        var client = CreateClient();

        var clientUser = await ClientLogin(client);

        var result = await client.DeleteAsync("/admin/orders/cart");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task FinalizeOrder_WithItemsInCartAndAdminUser_SavesOrderAndReturns200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var item1 = await SeedItem("item1", 10.00m);
        var item2 = await SeedItem("item2", 15.50m, "category2");

        await SeedCartItem(employeeUser.UserId, item1.ItemId, 3);
        await SeedCartItem(employeeUser.UserId, item2.ItemId, 2);

        var requestDto = new OrderCreateDto
        {
            LocationId = location.LocationId,
            IsPaid = false,
            PaymentMethod = PaymentMethod.Cash,
            OrderType = OrderType.DineIn,
            PhoneNumber = "123 123 123"
        };

        var timeBefore = DateTime.UtcNow;

        var result = await client.PostAsJsonAsync("/admin/orders", requestDto);

        var timeAfter = DateTime.UtcNow;

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var resultJson = await result.ReadJsonAsync<OrderResponseDto>();

        Assert.NotNull(resultJson);
        Assert.Equal(location.LocationId, resultJson.LocationId);
        Assert.Equal(item1.Price * 3 + item2.Price * 2, resultJson.Price);
        Assert.Equal(requestDto.PhoneNumber, resultJson.PhoneNumber);
        Assert.InRange(resultJson.OrderTime, timeBefore, timeAfter);
        Assert.Equal(
            new[] { item1.ItemId, item2.ItemId }.OrderBy(id => id),
            resultJson.Items.Select(item => item.ItemId).OrderBy(id => id)
        );
        Assert.Equal(requestDto.PaymentMethod, resultJson.PaymentMethod);
        Assert.Equal(requestDto.IsPaid, resultJson.IsPaid);
        Assert.Equal(OrderStatus.Pending, resultJson.OrderStatus);
        Assert.Equal(requestDto.OrderType, resultJson.OrderType);
        Assert.Null(resultJson.Delivery);

        var cartCount = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.CartItems
                .AsNoTracking()
                .Where(ci => ci.UserId == employeeUser.UserId)
                .CountAsync()
        );

        Assert.Equal(0, cartCount);

        var createdOrder = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == resultJson.OrderId)
                .SingleOrDefaultAsync()
        );

        Assert.NotNull(createdOrder);
        Assert.Equal(location.LocationId, createdOrder.LocationId);
        Assert.Equal(item1.Price * 3 + item2.Price * 2, createdOrder.Price);
        Assert.Equal(requestDto.PhoneNumber, createdOrder.PhoneNumber);
        Assert.InRange(createdOrder.OrderTime, timeBefore, timeAfter);
        Assert.Equal(requestDto.PaymentMethod, createdOrder.PaymentMethod);
        Assert.Equal(requestDto.IsPaid, createdOrder.IsPaid);
        Assert.Equal(OrderStatus.Pending, createdOrder.Status);
        Assert.Equal(requestDto.OrderType, createdOrder.OrderType);
        Assert.Null(createdOrder.Delivery);

        var createdOrderItems = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.OrderId == createdOrder.OrderId)
                .Select(oi => oi.ItemId)
                .OrderBy(id => id)
                .ToListAsync()
        );

        Assert.Equal(2, createdOrderItems.Count);
        Assert.Equal(
            new[] { item1.ItemId, item2.ItemId }.OrderBy(id => id),
            createdOrderItems
        );
    }

    [Fact]
    public async Task FinalizeOrder_WithItemsInCartAndNonAdminEmployeeUser_SavesOrderAndReturns200()
    {
        var client = CreateClient();

        var employeeUser = await EmployeeLogin(client, false);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var item1 = await SeedItem("item1", 10.00m);

        await SeedCartItem(employeeUser.UserId, item1.ItemId, 3);

        var requestDto = new OrderCreateDto
        {
            LocationId = location.LocationId,
            IsPaid = false,
            PaymentMethod = PaymentMethod.Cash,
            OrderType = OrderType.DineIn,
        };

        var result = await client.PostAsJsonAsync("/admin/orders", requestDto);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var resultJson = await result.ReadJsonAsync<OrderResponseDto>();

        Assert.NotNull(resultJson);

        var createdOrder = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == resultJson.OrderId)
                .AnyAsync()
        );

        Assert.True(createdOrder);
    }

    [Fact]
    public async Task FinalizeOrder_WithClient_Returns403()
    {
        var client = CreateClient();

        var clientUser = await ClientLogin(client);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var item1 = await SeedItem("item1", 10.00m);

        await SeedCartItem(clientUser.UserId, item1.ItemId, 3);

        var requestDto = new OrderCreateDto
        {
            LocationId = location.LocationId,
            IsPaid = false,
            PaymentMethod = PaymentMethod.Cash,
            OrderType = OrderType.DineIn,
        };

        var result = await client.PostAsJsonAsync("/admin/orders", requestDto);

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task FinalizeOrder_WithNoCartItems_Returns400()
    {
        var client = CreateClient();

        var user = await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var requestDto = new OrderCreateDto
        {
            LocationId = location.LocationId,
            IsPaid = false,
            PaymentMethod = PaymentMethod.Cash,
            OrderType = OrderType.DineIn,
        };

        var result = await client.PostAsJsonAsync("/admin/orders", requestDto);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task FinalizeOrder_WithInvalidLocationId_Returns404()
    {
        var client = CreateClient();

        var user = await EmployeeLogin(client, true);

        var item1 = await SeedItem("item1", 10.00m);

        await SeedCartItem(user.UserId, item1.ItemId, 3);

        var requestDto = new OrderCreateDto
        {
            LocationId = int.MaxValue,
            IsPaid = false,
            PaymentMethod = PaymentMethod.Cash,
            OrderType = OrderType.DineIn,
        };

        var result = await client.PostAsJsonAsync("/admin/orders", requestDto);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task FinalizeOrder_WithInvalidClientId_Returns404()
    {
        var client = CreateClient();

        var user = await EmployeeLogin(client, true);

        var item1 = await SeedItem("item1", 10.00m);

        await SeedCartItem(user.UserId, item1.ItemId, 3);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var requestDto = new OrderCreateDto
        {
            ClientId = int.MinValue,
            LocationId = location.LocationId,
            IsPaid = false,
            PaymentMethod = PaymentMethod.Cash,
            OrderType = OrderType.DineIn,
        };

        var result = await client.PostAsJsonAsync("/admin/orders", requestDto);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task FinalizeOrder_WithValidDeliveryData_CreatesDeliveryAndReturns200()
    {
        var client = CreateClient();

        var user = await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var item1 = await SeedItem("item1", 10.00m);

        await SeedCartItem(user.UserId, item1.ItemId, 3);

        var requestDto = new OrderCreateDto
        {
            LocationId = location.LocationId,
            IsPaid = false,
            PaymentMethod = PaymentMethod.Cash,
            OrderType = OrderType.Delivery,
            PhoneNumber = "123 123 123",
            City = "City 1",
            Address = "Address 1"
        };

        var result = await client.PostAsJsonAsync("/admin/orders", requestDto);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var resultJson = await result.ReadJsonAsync<OrderResponseDto>();
        Assert.Equal(requestDto.PhoneNumber, resultJson.PhoneNumber);
        Assert.NotNull(resultJson.Delivery);
        DeliveryResponseDto deliveryDto = resultJson.Delivery;

        Assert.Equal(requestDto.PhoneNumber, deliveryDto.PhoneNumber);
        Assert.Equal(requestDto.Address, deliveryDto.Address);
        Assert.Equal(requestDto.City, deliveryDto.City);

        var createdDelivery = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.Deliveries
                .AsNoTracking()
                .Where(d => d.OrderId == resultJson.OrderId)
                .SingleOrDefaultAsync()
        );

        Assert.NotNull(createdDelivery);
        Assert.Equal(resultJson.OrderId, createdDelivery.OrderId);
        Assert.Equal(requestDto.Address, createdDelivery.Address);
        Assert.Equal(requestDto.City, createdDelivery.City);
    }

    [Theory]
    [InlineData("", "TestCity", "TestAddress")]
    [InlineData("123 123 123", "", "TestAddress")]
    [InlineData("123 123 123", "TestCity", "")]
    public async Task FinalizeOrder_WithInvalidDeliveryData_Returns400(
        string phoneNumber,
        string city,
        string address
    )
    {
        var client = CreateClient();

        var user = await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var item1 = await SeedItem("item1", 10.00m);

        var ordersBefore = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.Deliveries
                .AsNoTracking()
                .CountAsync()
        );

        await SeedCartItem(user.UserId, item1.ItemId, 3);

        var requestDto = new OrderCreateDto
        {
            LocationId = location.LocationId,
            IsPaid = false,
            PaymentMethod = PaymentMethod.Cash,
            OrderType = OrderType.Delivery,
            PhoneNumber = phoneNumber,
            City = city,
            Address = address
        };

        var result = await client.PostAsJsonAsync("/admin/orders", requestDto);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);


        var ordersAfter = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.Deliveries
                .AsNoTracking()
                .CountAsync()
        );

        Assert.Equal(ordersBefore, ordersAfter);
    }

    [Fact]
    public async Task GetAllOrders_WithExistingOrdersAndAdminUser_ReturnsAllOrdersAnd200()
    {
        var client = CreateClient();

        var user = await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item1 = await SeedItem("Item 1", 10.00m);
        var item2 = await SeedItem("Item 2", 15.00m, "C2");

        var order1 = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId);
        var order2 = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId, true);

        var result = await client.GetAsync("/admin/orders");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var resultJson = await result.ReadJsonAsync<List<OrderResponseDto>>();

        Assert.NotNull(resultJson);

        Assert.Equal(2, resultJson.Count);
        Assert.Equal(
            new[] { order1.OrderId, order2.OrderId }.OrderBy(id => id),
            resultJson.Select(o => o.OrderId)
            .OrderBy(id => id)
            .ToList()
        );
        Assert.Null(resultJson.Single(o => o.OrderId == order1.OrderId).Delivery);
        Assert.NotNull(resultJson.Single(o => o.OrderId == order2.OrderId).Delivery);
        Assert.All(resultJson, o => Assert.Equal("123 123 123", o.PhoneNumber));
        Assert.All(resultJson, o => Assert.Equal(2, o.Items.Count));
    }

    [Fact]
    public async Task GetAllOrders_WithExistingOrdersAndNonAdminEmployeeUser_ReturnsAllOrdersAnd200()
    {
        var client = CreateClient();

        var user = await EmployeeLogin(client, false);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item1 = await SeedItem("Item 1", 10.00m);
        var item2 = await SeedItem("Item 2", 15.00m, "C2");

        var order1 = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId);
        var order2 = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId, true);

        var result = await client.GetAsync("/admin/orders");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var resultJson = await result.ReadJsonAsync<List<OrderResponseDto>>();

        Assert.NotNull(resultJson);

        Assert.Equal(2, resultJson.Count);
    }

    [Fact]
    public async Task GetAllOrders_WithClientUser_Returns403()
    {
        var client = CreateClient();

        var user = await ClientLogin(client);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item1 = await SeedItem("Item 1", 10.00m);
        var item2 = await SeedItem("Item 2", 15.00m, "C2");

        var order1 = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId);
        var order2 = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId, true);

        var result = await client.GetAsync("/admin/orders");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetAllOrders_WithNoOrders_ReturnsEmptyListAnd200()
    {
        var client = CreateClient();

        var user = await EmployeeLogin(client, true);

        var result = await client.GetAsync("/admin/orders");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var resultJson = await result.ReadJsonAsync<List<OrderResponseDto>>();

        Assert.Empty(resultJson);
    }

    [Fact]
    public async Task FindOrderById_WithValidIdAndAdminUser_ReturnsOrderDataAnd200()
    {
        var client = CreateClient();

        var user = await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item1 = await SeedItem("Item 1", 10.00m);
        var item2 = await SeedItem("Item 2", 15.00m, "C2");

        var order = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId, true);
        var order2 = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId);

        var result = await client.GetAsync($"/admin/orders/{order.OrderId}");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var resultJson = await result.ReadJsonAsync<OrderResponseDto>();

        Assert.NotNull(resultJson);

        Assert.Equal(order.OrderId, resultJson.OrderId);
        Assert.Equal(order.LocationId, location.LocationId);
        Assert.Equal(order.Price, item1.Price + item2.Price);
        Assert.Equal(2, resultJson.Items.Count);
        Assert.Equal(
            new[] { item1.ItemId, item2.ItemId }.OrderBy(id => id),
            resultJson.Items.Select(oi => oi.ItemId).OrderBy(id => id).ToList()
        );
        Assert.Equal(OrderType.Delivery, resultJson.OrderType);
        Assert.False(resultJson.IsPaid);
        Assert.Equal(OrderStatus.Preparing, resultJson.OrderStatus);
        Assert.Equal("123 123 123", resultJson.PhoneNumber);
        Assert.NotNull(resultJson.Delivery);
        Assert.Equal("123 123 123", resultJson.Delivery.PhoneNumber);
        Assert.Equal("Test City 1", resultJson.Delivery.City);
        Assert.Equal("Test Address 1", resultJson.Delivery.Address);
    }

    [Fact]
    public async Task FindOrderById_WithValidIdAndNonAdminEmployeeUser_ReturnsOrderAnd200()
    {
        var client = CreateClient();

        var user = await EmployeeLogin(client, false);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item1 = await SeedItem("Item 1", 10.00m);
        var item2 = await SeedItem("Item 2", 15.00m, "C2");

        var order = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId, true);
        var order2 = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId);

        var result = await client.GetAsync($"/admin/orders/{order.OrderId}");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var resultJson = await result.ReadJsonAsync<OrderResponseDto>();

        Assert.NotNull(resultJson);
    }

    [Fact]
    public async Task FindOrderById_WithClientUser_Returns403()
    {
        var client = CreateClient();

        var user = await ClientLogin(client);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item1 = await SeedItem("Item 1", 10.00m);
        var item2 = await SeedItem("Item 2", 15.00m, "C2");

        var order = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId, true);
        var order2 = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId);

        var result = await client.GetAsync($"/admin/orders/{order.OrderId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task FindOrderById_WithInvalidId_Returns404()
    {
        var client = CreateClient();

        var user = await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item1 = await SeedItem("Item 1", 10.00m);
        var item2 = await SeedItem("Item 2", 15.00m, "C2");

        var order = await SeedOrder(new List<MenuItem>() { item1, item2 }, location.LocationId, true);

        var result = await client.GetAsync($"/admin/orders/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_WithAdminUserAndValidData_UpdatesOnlyProvidedFields()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item = await SeedItem("Item 1", 10.00m);
        var order = await SeedOrder(new List<MenuItem>() { item }, location.LocationId, true);

        var dto = new OrderUpdateDto
        {
            PaymentMethod = PaymentMethod.Card,
            IsPaid = true,
            Status = OrderStatus.Ready,
            PhoneNumber = "999 999 999"
        };

        var result = await client.PatchAsJsonAsync($"/admin/orders/{order.OrderId}", dto);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var body = await result.ReadJsonAsync<OrderResponseDto>();

        Assert.NotNull(body);
        Assert.Equal(order.OrderId, body.OrderId);
        Assert.Equal(OrderStatus.Ready, body.OrderStatus);
        Assert.Equal("999 999 999", body.PhoneNumber);
        Assert.Equal(PaymentMethod.Card, body.PaymentMethod);
        Assert.True(body.IsPaid);
        Assert.Equal(OrderType.Delivery, body.OrderType);
        Assert.Equal(item.Price, body.Price);
        Assert.Single(body.Items);
        Assert.NotNull(body.Delivery);
        Assert.Equal("Test City 1", body.Delivery.City);
        Assert.Equal("Test Address 1", body.Delivery.Address);

        var persisted = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.Orders
                .AsNoTracking()
                .Include(o => o.Delivery)
                .SingleAsync(o => o.OrderId == order.OrderId)
        );

        Assert.Equal(OrderStatus.Ready, persisted.Status);
        Assert.Equal("999 999 999", persisted.PhoneNumber);
        Assert.Equal(PaymentMethod.Card, persisted.PaymentMethod);
        Assert.True(persisted.IsPaid);
        Assert.Equal("Test City 1", persisted.Delivery!.City);
        Assert.Equal("Test Address 1", persisted.Delivery!.Address);
    }

    [Fact]
    public async Task UpdateOrder_WithAdminUserAndDeliveryData_UpdatesDelivery()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item = await SeedItem("Item 1", 10.00m);
        var order = await SeedOrder(new List<MenuItem>() { item }, location.LocationId, true);

        var dto = new OrderUpdateDto
        {
            City = "New City",
            Address = "New Address"
        };

        var result = await client.PatchAsJsonAsync($"/admin/orders/{order.OrderId}", dto);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var body = await result.ReadJsonAsync<OrderResponseDto>();

        Assert.NotNull(body);
        Assert.NotNull(body.Delivery);
        Assert.Equal("New City", body.Delivery.City);
        Assert.Equal("New Address", body.Delivery.Address);
        Assert.Equal("123 123 123", body.Delivery.PhoneNumber);
        Assert.Equal("123 123 123", body.PhoneNumber);

        var persistedDelivery = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.Deliveries
                .AsNoTracking()
                .SingleAsync(d => d.OrderId == order.OrderId)
        );

        Assert.Equal("New City", persistedDelivery.City);
        Assert.Equal("New Address", persistedDelivery.Address);
    }

    [Fact]
    public async Task UpdateOrder_WithNonAdminEmployeeUser_Returns200()
    {
        var client = CreateClient();

        await EmployeeLogin(client, false);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item = await SeedItem("Item 1", 10.00m);
        var order = await SeedOrder(new List<MenuItem>() { item }, location.LocationId, true);

        var dto = new OrderUpdateDto { Status = OrderStatus.Ready };

        var result = await client.PatchAsJsonAsync($"/admin/orders/{order.OrderId}", dto);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var body = await result.ReadJsonAsync<OrderResponseDto>();

        Assert.NotNull(body);
        Assert.Equal(OrderStatus.Ready, body.OrderStatus);
    }

    [Fact]
    public async Task UpdateOrder_WithClientUser_Returns403()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item = await SeedItem("Item 1", 10.00m);
        var order = await SeedOrder(new List<MenuItem>() { item }, location.LocationId);

        var result = await client.PatchAsJsonAsync(
            $"/admin/orders/{order.OrderId}",
            new OrderUpdateDto { Status = OrderStatus.Ready });

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_WithInvalidId_Returns404()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var result = await client.PatchAsJsonAsync(
            $"/admin/orders/{int.MaxValue}",
            new OrderUpdateDto { Status = OrderStatus.Ready });

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_WithEmptyDto_Returns400()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item = await SeedItem("Item 1", 10.00m);
        var order = await SeedOrder(new List<MenuItem>() { item }, location.LocationId, true);

        var result = await client.PatchAsJsonAsync(
            $"/admin/orders/{order.OrderId}",
            new OrderUpdateDto());

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_WithWhitespaceOnlyStrings_Returns400()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item = await SeedItem("Item 1", 10.00m);
        var order = await SeedOrder(new List<MenuItem>() { item }, location.LocationId, true);

        var dto = new OrderUpdateDto
        {
            PhoneNumber = "   ",
            City = "   ",
            Address = "   "
        };

        var result = await client.PatchAsJsonAsync($"/admin/orders/{order.OrderId}", dto);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_WithWhitespaceFieldsAndValidField_IgnoresWhitespaceAndReturns200()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item = await SeedItem("Item 1", 10.00m);
        var order = await SeedOrder(new List<MenuItem>() { item }, location.LocationId, true);

        var dto = new OrderUpdateDto
        {
            Status = OrderStatus.Ready,
            PhoneNumber = "   ",
            City = "   ",
            Address = "   "
        };

        var result = await client.PatchAsJsonAsync($"/admin/orders/{order.OrderId}", dto);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var body = await result.ReadJsonAsync<OrderResponseDto>();

        Assert.NotNull(body);
        Assert.Equal(OrderStatus.Ready, body.OrderStatus);
        Assert.Equal("123 123 123", body.PhoneNumber);
        Assert.NotNull(body.Delivery);
        Assert.Equal("Test City 1", body.Delivery.City);
        Assert.Equal("Test Address 1", body.Delivery.Address);
    }

    [Fact]
    public async Task UpdateOrder_WithDeliveryDataOnNonDeliveryOrder_Returns400()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item = await SeedItem("Item 1", 10.00m);
        var order = await SeedOrder(new List<MenuItem>() { item }, location.LocationId);

        var result = await client.PatchAsJsonAsync(
            $"/admin/orders/{order.OrderId}",
            new OrderUpdateDto { City = "Some City", Address = "Some Address" });

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var persisted = await DatabaseHelper.ExecuteAsync(
            _factory,
            async context => await context.Orders
                .AsNoTracking()
                .SingleAsync(o => o.OrderId == order.OrderId)
        );

        Assert.Equal("123 123 123", persisted.PhoneNumber);
    }

    [Fact]
    public async Task UpdateOrder_WithWhitespaceDeliveryDataOnNonDeliveryOrder_IgnoresWhitespaceAndReturns200()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            ctx => LocationSeeder.SeedLocation(ctx)
        );

        var item = await SeedItem("Item 1", 10.00m);
        var order = await SeedOrder(new List<MenuItem>() { item }, location.LocationId);

        var dto = new OrderUpdateDto
        {
            Status = OrderStatus.Ready,
            City = "   ",
            Address = "   "
        };

        var result = await client.PatchAsJsonAsync($"/admin/orders/{order.OrderId}", dto);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var body = await result.ReadJsonAsync<OrderResponseDto>();

        Assert.NotNull(body);
        Assert.Equal(OrderStatus.Ready, body.OrderStatus);
        Assert.Null(body.Delivery);
    }
}
