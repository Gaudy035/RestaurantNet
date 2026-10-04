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

    public async Task<Result<IEnumerable<MenuItemResponseDto>>> GetMenuItems(bool isAdmin, string? param)
    {
        var query = _context.MenuItems.AsNoTracking().AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(mi => mi.IsAvailable);
        }

        if (!string.IsNullOrWhiteSpace(param))
        {
            var par = $"%{param.ToLower()}%";
            query = query.Where(mi => EF.Functions.Like(mi.Name.ToLower(), par));
        }

        var foundItems = await query.Select(mi => new MenuItemResponseDto
        {
            ItemId = mi.ItemId,
            CategoryId = mi.CategoryId,
            Name = mi.Name,
            Price = mi.Price,
            ImageUrl = mi.ImageUrl,
            IsAvailable = mi.IsAvailable,
            IsPinned = mi.IsPinned
        }).ToListAsync();
        
        return Result<IEnumerable<MenuItemResponseDto>>.Success(foundItems);
    }

    public async Task<Result<MenuItemResponseDto>> GetMenuItemById(int itemId)
    {
        var menuItem = await _context.MenuItems.AsNoTracking()
            .FirstOrDefaultAsync(mi => mi.ItemId == itemId);

        if (menuItem == null)
        {
            return Result<MenuItemResponseDto>.Fail(ErrorCode.MenuItemNotFound);
        }

        return Result<MenuItemResponseDto>.Success(new MenuItemResponseDto
        {
            ItemId = menuItem.ItemId,
            CategoryId = menuItem.CategoryId,
            Name = menuItem.Name,
            Price = menuItem.Price,
            ImageUrl = menuItem.ImageUrl,
            IsAvailable = menuItem.IsAvailable,
            IsPinned = menuItem.IsPinned
        });
    }

    public async Task<Result<MenuItemResponseDto>> UpdateMenuItem(int itemId, MenuItemUpdateDto dto)
    {
        if (dto.IsEmpty)
        {
            return Result<MenuItemResponseDto>.Fail(ErrorCode.UpdateBodyEmpty);
        }
        
        var menuItem = await _context.MenuItems.FindAsync(itemId);

        if (menuItem == null)
        {
            return Result<MenuItemResponseDto>.Fail(ErrorCode.MenuItemNotFound);
        }
        
        var categoryId = dto.CategoryId;

        if (categoryId.HasValue)
        {
            var categoryExists = await _context.Categories.AsNoTracking()
                .AnyAsync(c => c.CategoryId == categoryId.Value);

            if (!categoryExists)
            {
                return Result<MenuItemResponseDto>.Fail(ErrorCode.CategoryNotFound);
            }
        }

        if (categoryId.HasValue) menuItem.CategoryId = categoryId.Value;
        if (dto.Price != null) menuItem.Price = dto.Price!.Value;
        if (dto.IsAvailable != null) menuItem.IsAvailable = dto.IsAvailable!.Value;
        if (dto.IsPinned != null) menuItem.IsPinned = dto.IsPinned!.Value;
        if (dto.ImageUrl != null)
        {
            menuItem.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl;
        }

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
                ItemId = menuItem.ItemId,
                CategoryId = menuItem.CategoryId,
                Name = menuItem.Name,
                Price = menuItem.Price,
                ImageUrl = menuItem.ImageUrl,
                IsAvailable = menuItem.IsAvailable,
                IsPinned = menuItem.IsPinned
            }
        );
    }
}