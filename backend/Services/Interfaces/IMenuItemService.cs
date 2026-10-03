using backend.DTOs.MenuItems;
using backend.Services.Errors;

namespace backend.Services.Interfaces;

public interface IMenuItemService
{
    Task<Result<MenuItemResponseDto>> CreateMenuItem(MenuItemCreateDto dto);
}