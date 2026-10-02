using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_ingredient")]
public class Ingredient
{
    [Key]
    [Column("ingredient_id")]
    public int IngredientId { get; init; }

    [Required]
    [MaxLength(128)]
    [Column("name")]
    public string Name { get; init; } = string.Empty;

    [MaxLength(50)]
    [Column("allergen")]
    public string? Allergen { get; init; }

    public ICollection<ItemIngredient> ItemIngredients { get; init; } = new List<ItemIngredient>();
}