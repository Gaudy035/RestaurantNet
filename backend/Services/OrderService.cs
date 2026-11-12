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
}