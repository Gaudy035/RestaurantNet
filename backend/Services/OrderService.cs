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

    private async Task ClearCartItems(int userId)
    {
        var items = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        _context.CartItems.RemoveRange(items);
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

    public async Task<Result<CartResponseDto>> GetCart(int userId)
    {
        var cartItems = await _context.CartItems
            .AsNoTracking()
            .Include(ci => ci.MenuItem)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        return Result<CartResponseDto>.Success(new CartResponseDto
        {
            Items = cartItems.Select(ci => new CartItemResponseDto
            {
                ItemId = ci.ItemId,
                Name = ci.MenuItem.Name,
                Price = ci.MenuItem.Price,
                Quantity = ci.Quantity
            }).ToList(),
            TotalPrice = cartItems.Sum(ci => ci.MenuItem.Price * ci.Quantity)
        });
    }

    public async Task<Result> ClearCart(int userId)
    {
        await ClearCartItems(userId);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Result.Fail(ErrorCode.DbOperationFailed);
        }

        return Result.Success();
    }

    public async Task<Result<OrderResposneDto>> FinalizeOrder(int userId, OrderCreateDto dto)
    {
        if (
            string.IsNullOrWhiteSpace(dto.Address)
            || string.IsNullOrWhiteSpace(dto.City)
            || string.IsNullOrWhiteSpace(dto.PhoneNumber)
        )
        {
            return Result<OrderResposneDto>.Fail(ErrorCode.InvalidDeliveryData);
        }

        var cartItems = await _context.CartItems
            .AsNoTracking()
            .Include(ci => ci.MenuItem)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return Result<OrderResposneDto>.Fail(ErrorCode.CartEmpty);
        }

        var order = new Order
        {
            ClientId = dto.ClientId,
            LocationId = dto.LocationId,
            Price = cartItems.Sum(ci => ci.MenuItem.Price * ci.Quantity),
            OrderTime = DateTime.UtcNow,
            PhoneNumber = dto.PhoneNumber,
            PaymentMethod = dto.PaymentMethod,
            IsPaid = dto.IsPaid,
            Status = dto.OrderStatus,
            OrderType = dto.OrderType
        };

        await _context.Orders.AddAsync(order);

        if (dto.OrderType == OrderType.Delivery)
        {
            var delivery = new Delivery
            {
                Order = order,
                Address = dto.Address,
                City = dto.City,
            };

            await _context.Deliveries.AddAsync(delivery);
        }

        await ClearCartItems(userId);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Result<OrderResposneDto>.Fail(ErrorCode.DbOperationFailed);
        }

        var responseDto = new OrderResposneDto
        {
            OrderId = order.OrderId,
            ClientId = order.ClientId,
            LocationId = order.LocationId,
            Price = order.Price,
            OrderTime = order.OrderTime,
            Items = cartItems.Select(ci => new CartItemResponseDto
            {
                ItemId = ci.ItemId,
                Name = ci.MenuItem.Name,
                Price = ci.MenuItem.Price,
                Quantity = ci.Quantity
            }).ToList(),
            PaymentMethod = order.PaymentMethod,
            IsPaid = order.IsPaid,
            OrderStatus = order.Status,
            OrderType = order.OrderType
        };

        if (order.OrderType == OrderType.Delivery)
        {
            responseDto.Delivery = new DeliveryResponseDto
            {
                Address = dto.Address,
                City = dto.City,
                PhoneNumber = order.PhoneNumber
            };
        }

        return Result<OrderResposneDto>.Success(responseDto);
    }
}