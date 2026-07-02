using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_item_ingredient")]
public class ItemIngredient
{
    [Column("item_id")]
    public int ItemId { get; set; }
    
    [Column("ingredient_id")]
    public int IngredientId { get; set; }

    public MenuItem MenuItem { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}