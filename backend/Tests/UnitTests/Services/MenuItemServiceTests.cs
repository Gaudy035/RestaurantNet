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
    public async Task UpdateMenuItem_WithIncorrectCategoryId_ReturnsCategoryNotFoundError()
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
}