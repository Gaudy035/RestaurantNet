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

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("{categoryId:int}")]
    public async Task<IActionResult> GetCategoryById([FromRoute] int categoryId)
    {
        var result = await _categoryService.GetCategoryById(categoryId);
        
        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("{categoryId:int}/items")]
    public async Task<IActionResult> GetCategoryMenuItems([FromRoute] int categoryId)
    {
        var isAdmin = User.IsInRole("Admin");
        
        var result = await _categoryService.GetCategoryItems(categoryId, isAdmin);
        
        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{categoryId:int}")]
    public async Task<IActionResult> DeleteCategory([FromRoute] int categoryId)
    {
        var result = await _categoryService.DeleteCategory(categoryId);
        
        return result.ToActionResult(this);
    }
}