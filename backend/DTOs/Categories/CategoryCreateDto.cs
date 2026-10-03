using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Categories;

public class CategoryCreateDto
{
    [Required]
    [MaxLength(50)]
    public string CategoryName { get; init; } =  string.Empty;
}