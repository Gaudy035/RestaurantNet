using backend.Data;
using backend.Data.Entities;
using backend.DTOs.MenuItems;
using backend.Services.Errors;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class MenuItemService: IMenuItemService
{
    private readonly AppDbContext _context;
    
    public MenuItemService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MenuItemResponseDto>> CreateMenuItem(MenuItemCreateDto dto)
    {
        var categoryExists = await _context.Categories.AsNoTracking()
            .AnyAsync(c => c.CategoryId == dto.CategoryId);

        if (!categoryExists)
        {
            return Result<MenuItemResponseDto>.Fail(ErrorCode.CategoryNotFound);
        }

        var newMenuItem = new MenuItem
        {
            Name = dto.Name,
            CategoryId = dto.CategoryId,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            IsAvailable = dto.IsAvailable,
            IsPinned = dto.IsPinned
        };
        
        _context.MenuItems.Add(newMenuItem);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Result<MenuItemResponseDto>.Fail(ErrorCode.DbOperationFailed);
        }

        return Result<MenuItemResponseDto>.Success(new MenuItemResponseDto
        {
            ItemId = newMenuItem.ItemId,
            CategoryId = newMenuItem.CategoryId,
            Name = newMenuItem.Name,
            Price = newMenuItem.Price,
            ImageUrl = newMenuItem.ImageUrl,
            IsAvailable = newMenuItem.IsAvailable,
            IsPinned = newMenuItem.IsPinned
        });
    }
}