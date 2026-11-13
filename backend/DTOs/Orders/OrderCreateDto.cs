using backend.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Orders;

public class OrderCreateDto
{
    [Required]
    public int LocationId { get; init; }

    public int? ClientId { get; init; }

    public bool IsPaid { get; init; }

    [Required]
    public PaymentMethod PaymentMethod { get; init; }

    [Required]
    public OrderType OrderType { get; init; }

    public string PhoneNumber { get; init; } = string.Empty;

    public string? City { get; init; }

    public string? Address { get; init; }

    public OrderStatus OrderStatus { get; init; } = OrderStatus.Pending;
}
