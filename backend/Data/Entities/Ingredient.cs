using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_ingredient")]
public class Ingredient
{
    [Key]
    [Column("ingredient_id")]
    public int IngredientId { get; set; }

    [Required]
    [MaxLength(128)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("allergen")]
    public string? Allergen { get; set; }

    public ICollection<ItemIngredient> ItemIngredients { get; set; } = new List<ItemIngredient>();
}