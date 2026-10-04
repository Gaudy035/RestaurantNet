using backend.DTOs.Categories;
using backend.DTOs.MenuItems;
using backend.Services.Errors;

namespace backend.Services.Interfaces;

public interface ICategoryService
{
    Task<Result<CategoryResponseDto>> CreateCategory(CategoryCreateDto dto);
    
    Task<Result<IEnumerable<CategoryResponseDto>>> GetCategories();
    
    Task<Result<CategoryResponseDto>> GetCategoryById(int categoryId);
    
    Task<Result<IEnumerable<MenuItemResponseDto>>> GetCategoryItems(int categoryId);
    
    Task<Result> DeleteCategory(int categoryId);
}