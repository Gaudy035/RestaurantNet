using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Locations;

public class LocationCreateDto
{
    [Required]
    [MaxLength(50)]
    public string City { get; init; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Address { get; init; } = string.Empty;
}