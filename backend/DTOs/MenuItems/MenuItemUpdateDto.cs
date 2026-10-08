using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.DTOs.MenuItems;

public class MenuItemUpdateDto
{
    public int? CategoryId { get; init; }
    
    [Range(typeof(decimal), "0", "10000")]
    public decimal? Price { get; init; }
    
    public bool? IsAvailable { get; init; }
    
    public bool? IsPinned { get; init; }
    
    [MaxLength(2048)]
    public string? ImageUrl { get; init; }
    
    [JsonIgnore]
    public bool IsEmpty => CategoryId is null 
        && Price is null 
        && IsAvailable is null 
        && IsPinned is null
        && ImageUrl is null;
}