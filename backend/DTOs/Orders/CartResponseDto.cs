namespace backend.DTOs.Orders;

public class CartResponseDto
{
    public List<CartItemResponseDto> Items { get; init; } = new List<CartItemResponseDto>();

    public decimal TotalPrice { get; init; }
}
