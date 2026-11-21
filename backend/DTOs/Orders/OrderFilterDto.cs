namespace backend.DTOs.Orders;

public class OrderFilterDto
{
    public DateOnly? Date { get; init; }

    public int? LocationId { get; init; }
}
