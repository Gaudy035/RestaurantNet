using backend.DTOs.MenuItems;
using backend.Services.Errors;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Admin;

[ApiController]
[Route("admin/menuitems")]
public class AdminMenuItemController: ControllerBase
{
    private readonly IMenuItemService _menuItemService;

    public AdminMenuItemController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("")]
    public async Task<IActionResult> CreateMenuItem([FromBody] MenuItemCreateDto dto)
    {
        var result = await _menuItemService.CreateMenuItem(dto);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet()]
    public async Task<IActionResult> GetMenuItems([FromQuery] string? param)
    {
        var isAdmin = User.IsInRole("Admin");
        
        var result = await _menuItemService.GetMenuItems(isAdmin, param);
        
        return result.ToActionResult(this);
    }
    
    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("{itemId:int}")]
    public async Task<IActionResult> GetMenuItemById(int itemId)
    {
        var result = await _menuItemService.GetMenuItemById(itemId);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{itemId:int}")]
    public async Task<IActionResult> UpdateMenuItem(int itemId, [FromBody] MenuItemUpdateDto dto)
    {
        var result = await _menuItemService.UpdateMenuItem(itemId, dto);
        
        return result.ToActionResult(this);
    }
}