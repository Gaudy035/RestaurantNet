using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Entities;

[Table("t_menu_item")]
public class MenuItem
{
    [Key]
    [Column("item_id")]
    public int ItemId { get; init; }

    [Required]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(128)]
    [Column("name")]
    public string Name { get; init; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0", "10000")]
    [Precision(7, 2)]
    [Column("price")]
    public decimal Price { get; set; }

    [MaxLength(2048)]
    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("is_available")]
    public bool IsAvailable { get; set; }

    [Column("is_pinned")]
    public bool IsPinned { get; set; }

    public Category Category { get; set; } = null!;

    public ICollection<ItemIngredient> ItemIngredients { get; init; } = new List<ItemIngredient>();

    public ICollection<OrderItem> OrderItems { get; init; } = new List<OrderItem>();
}