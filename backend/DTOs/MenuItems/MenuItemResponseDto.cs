namespace backend.DTOs.MenuItems;

public class MenuItemResponseDto
{
    public int ItemId { get; init; }
    
    public int CategoryId { get; init; }
    
    public string Name { get; init; } = string.Empty;
    
    public decimal Price { get; init; }
    
    public string? ImageUrl { get; init; }
    
    public bool IsAvailable { get; init; }
    
    public bool IsPinned { get; init; }
}