using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_menu_item")]
public class MenuItem
{
    [Key]
    [Column("item_id")]
    public int ItemId { get; set; }

    [Required]
    [MaxLength(128)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    [Column("price")]
    public double Price { get; set; }

    [Column("is_available")]
    public bool IsAvailable { get; set; }

    public ICollection<ItemIngredient> ItemIngredients { get; set; } = new List<ItemIngredient>();

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}