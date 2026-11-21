using backend.Data.Entities;
using System.Text.Json.Serialization;

namespace backend.DTOs.Orders;

public class OrderUpdateDto
{
    public PaymentMethod? PaymentMethod { get; init; }

    public bool? IsPaid { get; init; }

    public OrderStatus? Status { get; init; }

    public string? PhoneNumber { get; init; }

    public string? City { get; init; }
    
    public string? Address { get; init; }

    [JsonIgnore]
    public bool IsEmpty => PaymentMethod is null
        && IsPaid is null
        && Status is null
        && string.IsNullOrWhiteSpace(PhoneNumber)
        && string.IsNullOrWhiteSpace(City)
        && string.IsNullOrWhiteSpace(Address);
}
