using backend.DTOs.Categories;
using backend.Services.Errors;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Admin;

[ApiController]
[Route("admin/categories")]
public class AdminCategoryController: ControllerBase
{
    private readonly ICategoryService _categoryService;

    public AdminCategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
    {
        var result = await _categoryService.CreateCategory(dto);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _categoryService.GetCategories();
        
        return result.ToActionResult(this);
    }
}