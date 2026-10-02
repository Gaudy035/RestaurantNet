namespace backend.DTOs.Locations;

public class LocationResponseDto
{
    public int LocationId { get; init; }

    public string City { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;
}