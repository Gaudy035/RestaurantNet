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
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(128)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    [Column("price")]
    public double Price { get; set; }

    [MaxLength(2048)]
    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("is_available")]
    public bool IsAvailable { get; set; }

    [Column("is_pinned")]
    public bool IsPinned { get; set; }

    public Category Category { get; set; } = null!;

    public ICollection<ItemIngredient> ItemIngredients { get; set; } = new List<ItemIngredient>();

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}