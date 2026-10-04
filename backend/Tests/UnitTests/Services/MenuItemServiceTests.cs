using backend.Data;
using backend.DTOs.MenuItems;
using backend.Services;
using Microsoft.Data.Sqlite;
using UnitTests.Helpers;

namespace UnitTests.Services;

public class MenuItemServiceTests: IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly MenuItemService _menuItemService;

    public MenuItemServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
        _menuItemService = new MenuItemService(_context);
    }
    
    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateMenuItem_WithValidData_ReturnsMenuItem()
    {
        var category = await MenuSeeder.SeedCategory(_context);

        var dto = new MenuItemCreateDto
        {
            CategoryId = category.CategoryId,
            Name = "Test",
            ImageUrl = "TestUrl",
            IsAvailable = true,
            IsPinned = false,
            Price = 25
        };
        
        var result = await _menuItemService.CreateMenuItem(dto);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.CategoryId, result.Data.CategoryId);
        Assert.Equal(dto.Name, result.Data.Name);
        Assert.Equal(dto.ImageUrl, result.Data.ImageUrl);
        Assert.Equal(dto.IsAvailable, result.Data.IsAvailable);
        Assert.Equal(dto.IsPinned, result.Data.IsPinned);
        Assert.Equal(dto.Price, result.Data.Price);
    }

    [Fact]
    public async Task CreateMenuItem_WithIncorrectCategoryId_ReturnsCategoryNotFoundError()
    {
        var dto = new MenuItemCreateDto
        {
            CategoryId = int.MaxValue,
            Name = "Test",
            ImageUrl = "TestUrl",
            IsAvailable = true,
            IsPinned = false,
            Price = 25
        };
        
        var result = await _menuItemService.CreateMenuItem(dto);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
    }

    [Fact]
    public async Task GetMenuItems_WithAdmin_ReturnsAllMenuItems()
    {
        var category = await MenuSeeder.SeedCategory(_context);
        
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "Item1", isAvailable:true);
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "Item2", isAvailable:false);

        var result = await _menuItemService.GetMenuItems(isAdmin: true, param: null);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task GetMenuItems_WithNonAdmin_ReturnsAvailableMenuItems()
    {
        var category = await MenuSeeder.SeedCategory(_context);
        
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "Item1", isAvailable:true);
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "Item2", isAvailable:false);

        var result = await _menuItemService.GetMenuItems(isAdmin: false, param: null);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
    }

    [Fact]
    public async Task GetMenuItems_WithNoItems_ReturnsEmptyList()
    {
        var result = await _menuItemService.GetMenuItems(isAdmin: true, param: null);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetMenuItems_WithAdminAndParam_ReturnsMatchingMenuItems()
    {
        var category = await MenuSeeder.SeedCategory(_context);
        
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "ShouldBeFound", isAvailable:true);
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "ShouldAlsoBeFound", isAvailable:false);
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "ShouldNot", isAvailable:true);

        var result = await _menuItemService.GetMenuItems(isAdmin: true, param: "befo");
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
        Assert.Contains(result.Data, x => x.Name == "ShouldBeFound");
        Assert.Contains(result.Data, x => x.Name == "ShouldAlsoBeFound");
        Assert.DoesNotContain(result.Data, x => x.Name == "ShouldNot");
    }
    
    [Fact]
    public async Task GetMenuItems_WithNonAdminAndParam_ReturnsMatchingAvailableMenuItems()
    {
        var category = await MenuSeeder.SeedCategory(_context);
        
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "ShouldBeFound", isAvailable:true);
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "ShouldAlsoBeFound", isAvailable:false);
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "ShouldNot", isAvailable:true);

        var result = await _menuItemService.GetMenuItems(isAdmin: false, param: "befo");
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Contains(result.Data, x => x.Name == "ShouldBeFound");
        Assert.DoesNotContain(result.Data, x => x.Name == "ShouldAlsoBeFound");
        Assert.DoesNotContain(result.Data, x => x.Name == "ShouldNot");
    }

    [Fact]
    public async Task GetMenuItems_WithParamAndNoMatches_ReturnsEmptyList()
    {
        var category = await MenuSeeder.SeedCategory(_context);
        
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "Item1", isAvailable:true);
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "Item2", isAvailable:false);
        await MenuSeeder.SeedMenuItem(_context, categoryId: category.CategoryId, name: "Item3", isAvailable:true);
        
        var result = await _menuItemService.GetMenuItems(isAdmin: true, param: "RandomParam");
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }
    
    [Fact]
    public async Task GetMenuItemById_WithCorrectId_ReturnsMenuItem()
    {
        var category = await MenuSeeder.SeedCategory(_context);

        var menuItem = await MenuSeeder.SeedMenuItem(_context, category.CategoryId);

        var result = await _menuItemService.GetMenuItemById(menuItem.ItemId);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(menuItem.ItemId,  result.Data.ItemId);
        Assert.Equal(menuItem.CategoryId, result.Data.CategoryId);
        Assert.Equal(menuItem.Name, result.Data.Name);
        Assert.Equal(menuItem.ImageUrl, result.Data.ImageUrl);
        Assert.Equal(menuItem.IsAvailable, result.Data.IsAvailable);
        Assert.Equal(menuItem.IsPinned, result.Data.IsPinned);
    }
    
    [Fact]
    public async Task GetMenuItemById_WithInvalidId_ReturnsMenuItemNotFoundError()
    {
        var result = await _menuItemService.GetMenuItemById(int.MaxValue);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
    }
}