using backend.Data.Entities;

namespace backend.DTOs.Orders;

public class OrderResposneDto
{
    public int OrderId { get; init; }

    public int? ClientId { get; init; }

    public int LocationId { get; init; }

    public decimal Price { get; init; }

    public DateTime OrderTime { get; init; }

    public List<CartItemResponseDto> Items { get; init; } = new List<CartItemResponseDto>();

    public PaymentMethod PaymentMethod { get; init; }

    public bool IsPaid { get; init; }

    public OrderStatus OrderStatus { get; init; }

    public OrderType OrderType { get; init; }

    public DeliveryResponseDto? Delivery { get; set; }
}
