using backend.Data;
using backend.DTOs.Orders;
using backend.Services.Errors;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using backend.Data.Entities;

namespace backend.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CartItemResponseDto>> AddItemToCart(int userId, int itemId)
    {
        var menuItem = await _context.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(mi => mi.ItemId == itemId);
        
        if (menuItem == null)
        {
            return Result<CartItemResponseDto>.Fail(ErrorCode.MenuItemNotFound);
        }

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ItemId == itemId);

        if (cartItem != null)
        {
            cartItem.Quantity++;
        }
        else
        {
            cartItem = new CartItem
            {
                UserId = userId,
                ItemId = itemId,
                Quantity = 1
            };
            _context.CartItems.Add(cartItem);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Result<CartItemResponseDto>.Fail(ErrorCode.DbOperationFailed);
        }

        return Result<CartItemResponseDto>.Success(new CartItemResponseDto
        {
            ItemId = cartItem.ItemId,
            Name = menuItem.Name,
            Price = menuItem.Price,
            Quantity = cartItem.Quantity
        });
    }

    public async Task<Result<CartItemResponseDto?>> RemoveItemFromCart(int userId, int itemId)
    {
        var cartItem = await _context.CartItems
            .Include(ci => ci.MenuItem)
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ItemId == itemId);

        if (cartItem == null)
        {
            return Result<CartItemResponseDto?>.Fail(ErrorCode.CartItemNotFound);
        }

        var deleted = cartItem.Quantity <= 1;

        if (deleted)
        {
            _context.CartItems.Remove(cartItem);
        }
        else
        {
            cartItem.Quantity--;
        }

        try
        {
            await _context.SaveChangesAsync();

        }
        catch (DbUpdateException)
        {
            return Result<CartItemResponseDto?>.Fail(ErrorCode.DbOperationFailed);
        }

        if (deleted)
        {
            return Result<CartItemResponseDto?>.Success(null);
        }
        else
        {
            return Result<CartItemResponseDto?>.Success(new CartItemResponseDto
            {
                ItemId = cartItem.ItemId,
                Name = cartItem.MenuItem.Name,
                Price = cartItem.MenuItem.Price,
                Quantity = cartItem.Quantity
            });
        }
    }
}