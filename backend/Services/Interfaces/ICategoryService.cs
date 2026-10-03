using backend.DTOs.Categories;
using backend.Services.Errors;

namespace backend.Services.Interfaces;

public interface ICategoryService
{
    Task<Result<CategoryResponseDto>> CreateCategory(CategoryCreateDto dto);
}