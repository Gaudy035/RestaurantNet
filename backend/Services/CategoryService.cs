using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Categories;
using backend.Services.Errors;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class CategoryService: ICategoryService
{
    private readonly AppDbContext _context;
    
    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CategoryResponseDto>> CreateCategory(CategoryCreateDto dto)
    {
        var categoryExists = await _context.Categories.AsNoTracking()
            .AnyAsync(c => c.CategoryName == dto.CategoryName);

        if (categoryExists)
        {
            return Result<CategoryResponseDto>.Fail(ErrorCode.CategoryAlreadyExists);
        }
        
        var newCategory = new Category
        {
            CategoryName = dto.CategoryName
        };

        _context.Categories.Add(newCategory);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Result<CategoryResponseDto>.Fail(ErrorCode.DbOperationFailed);
        }

        return Result<CategoryResponseDto>.Success(new CategoryResponseDto
        {
            CategoryId = newCategory.CategoryId,
            CategoryName = newCategory.CategoryName
        });
    }

    public async Task<Result<IEnumerable<CategoryResponseDto>>> GetCategories()
    {
        var categories = await _context.Categories.AsNoTracking()
            .Select(c => new CategoryResponseDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                }
            ).ToListAsync();
        
        return Result<IEnumerable<CategoryResponseDto>>.Success(categories);
    }

    public async Task<Result<CategoryResponseDto>> GetCategoryById(int categoryId)
    {
        var category = await _context.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        if (category == null)
        {
            return Result<CategoryResponseDto>.Fail(ErrorCode.CategoryNotFound);
        }

        return Result<CategoryResponseDto>.Success(new CategoryResponseDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName
        });
    }
}