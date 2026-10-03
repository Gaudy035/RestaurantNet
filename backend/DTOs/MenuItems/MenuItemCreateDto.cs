using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.MenuItems;

public class MenuItemCreateDto
{
    [Required]
    [MaxLength(128)]
    public string Name { get; init; } = string.Empty;
    
    [Required]
    public int CategoryId { get; init; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public double Price { get; init; }
    
    [Required]
    public bool IsAvailable { get; init; }
    
    [Required]
    public bool IsPinned { get; init; }
    
    [MaxLength(2048)]
    public string? ImageUrl { get; init; }
}