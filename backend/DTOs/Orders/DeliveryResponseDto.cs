namespace backend.DTOs.Orders;

public class DeliveryResponseDto
{
    public string PhoneNumber { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;
}
