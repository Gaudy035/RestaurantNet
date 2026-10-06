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
    public async Task CreateCategory_CreatesCategoryAndReturnsIt()
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
    public async Task CreateCategory_WithDuplicateName_ReturnsCategoryAlreadyExistsError()
    {
        await MenuSeeder.SeedCategory(_context);

        var dto = new CategoryCreateDto
        {
            CategoryName = "TestCategory"
        };
        
        var result = await _categoryService.CreateCategory(dto);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(409, result.Error.StatusCode);
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

    [Fact]
    public async Task GetCategoryById_WithValidId_ReturnsCategory()
    {
        var category = await MenuSeeder.SeedCategory(_context);
        
        var result = await _categoryService.GetCategoryById(category.CategoryId);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(category.CategoryId, result.Data.CategoryId);
        Assert.Equal(category.CategoryName, result.Data.CategoryName);
    }

    [Fact]
    public async Task GetCategoryById_WithInvalidId_ReturnsCategoryNotFoundError()
    {
        var result = await _categoryService.GetCategoryById(int.MaxValue);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
    }

    [Fact]
    public async Task GetCategoryItems_WithValidIdAndAdmin_ReturnsAllCategoryItems()
    {
        var category1 = await MenuSeeder.SeedCategory(_context);
        var category2 = await MenuSeeder.SeedCategory(_context, "Category2");
        
        var item1 = await MenuSeeder.SeedMenuItem(_context, categoryId: category1.CategoryId, name: "Item1", isAvailable: true);
        var item2 = await MenuSeeder.SeedMenuItem(_context, categoryId: category1.CategoryId, name: "Item2", isAvailable: true);
        var item3 = await MenuSeeder.SeedMenuItem(_context, categoryId: category1.CategoryId, name: "Item3", isAvailable: false);
        var item4 = await MenuSeeder.SeedMenuItem(_context, categoryId: category2.CategoryId, name: "Item4", isAvailable: true);
        
        var result = await _categoryService.GetCategoryItems(category1.CategoryId, false);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count());
        Assert.Contains(result.Data, x => x.Name == "Item1");
        Assert.Contains(result.Data, x => x.Name == "Item2");
        Assert.Contains(result.Data, x => x.Name == "Item3");
        Assert.DoesNotContain(result.Data, x => x.Name == "Item4");
    }

    [Fact]
    public async Task GetCategoryItems_WithValidIdAndNonAdmin_ReturnsAvailableCategoryItems()
    {
        var category1 = await MenuSeeder.SeedCategory(_context);
        var category2 = await MenuSeeder.SeedCategory(_context, "Category2");
        
        var item1 = await MenuSeeder.SeedMenuItem(_context, categoryId: category1.CategoryId, name: "Item1", isAvailable: true);
        var item2 = await MenuSeeder.SeedMenuItem(_context, categoryId: category1.CategoryId, name: "Item2", isAvailable: true);
        var item3 = await MenuSeeder.SeedMenuItem(_context, categoryId: category1.CategoryId, name: "Item3", isAvailable: false);
        var item4 = await MenuSeeder.SeedMenuItem(_context, categoryId: category2.CategoryId, name: "Item4", isAvailable: true);
        
        var result = await _categoryService.GetCategoryItems(category1.CategoryId, false);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
        Assert.Contains(result.Data, x => x.Name == "Item1");
        Assert.Contains(result.Data, x => x.Name == "Item2");
        Assert.DoesNotContain(result.Data, x => x.Name == "Item3");
        Assert.DoesNotContain(result.Data, x => x.Name == "Item4");
    }

    [Fact]
    public async Task GetCategoryItems_WithValidIdAndNoItems_ReturnsEmptyList()
    {
        var category = await MenuSeeder.SeedCategory(_context);
        
        var result = await _categoryService.GetCategoryItems(category.CategoryId, true);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetCategoryItems_WithInvalidId_ReturnsCategoryNotFoundError()
    {
        var result = await _categoryService.GetCategoryItems(int.MaxValue, true);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WithNoItemsAndValidId_ReturnsSuccess()
    {
        var category = await MenuSeeder.SeedCategory(_context);
        
        var result = await _categoryService.DeleteCategory(category.CategoryId);
        
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public async Task DeleteCategory_WithInvalidId_ReturnsCategoryNotFoundError()
    {
        var result = await _categoryService.DeleteCategory(int.MaxValue);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
    }
    
    [Fact]
    public async Task DeleteCategory_WithAssignedItems_ReturnsCategoryContainItemsError()
    {
        var category = await MenuSeeder.SeedCategory(_context);
        
        await MenuSeeder.SeedMenuItem(_context, category.CategoryId);
        
        var result = await _categoryService.DeleteCategory(category.CategoryId);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(409, result.Error.StatusCode);
    }
    
}