namespace backend.DTOs.Locations;

public class LocationResponseDto
{
    public int LocationId { get; set; }

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}