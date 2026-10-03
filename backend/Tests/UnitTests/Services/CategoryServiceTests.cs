using backend.Data;
using backend.DTOs.Categories;
using backend.Services;
using Microsoft.Data.Sqlite;
using UnitTests.Helpers;

namespace UnitTests.Services;

public class CategoryServiceTests: IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly CategoryService _categoryService;
    
    public CategoryServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
        _categoryService = new CategoryService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AddCategory_CreatesCategoryAndReturnIt()
    {
        var dto = new CategoryCreateDto
        {
            CategoryName = "TestCategory"
        };
        
        var result = await _categoryService.CreateCategory(dto);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.CategoryName, result.Data.CategoryName);
    }
    
    [Fact]
    public async Task GetCategories_ReturnsAllCategories()
    {
        await MenuSeeder.SeedCategory(_context);
        await MenuSeeder.SeedCategory(_context, categoryName: "Category2");
        
        var result = await _categoryService.GetCategories();
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
        Assert.Contains(result.Data, c => c.CategoryName == "TestCategory");
        Assert.Contains(result.Data, c => c.CategoryName == "Category2");
    }

    [Fact]
    public async Task GetCategories_WithNoCategories_ReturnsEmpty()
    {
        var result = await _categoryService.GetCategories();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }
}