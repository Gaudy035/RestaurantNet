using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_item_ingredient")]
public class ItemIngredient
{
    [Column("item_id")]
    public int ItemId { get; init; }
    
    [Column("ingredient_id")]
    public int IngredientId { get; init; }

    public MenuItem MenuItem { get; init; } = null!;
    public Ingredient Ingredient { get; init; } = null!;
}