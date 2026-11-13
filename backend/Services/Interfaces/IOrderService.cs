using backend.DTOs.Orders;
using backend.Services.Errors;

namespace backend.Services.Interfaces;

public interface IOrderService
{
    Task<Result<CartItemResponseDto>> AddItemToCart(int userId, int itemId);
    Task<Result<CartItemResponseDto?>> RemoveItemFromCart(int userId, int itemId);
    Task<Result<CartResponseDto>> GetCart(int userId);
    Task<Result> ClearCart(int userId);
    Task<Result<OrderResposneDto>> FinalizeOrder(int userId, OrderCreateDto dto);
}