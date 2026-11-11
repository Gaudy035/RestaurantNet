using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace backend.Data.Entities;

[Table("t_cart_item")]
public class CartItem
{
    [Column("user_id")]
    public int UserId { get; init; }

    [Column("item_id")]
    public int ItemId { get; init; }

    [Required]
    [Column("quantity")]
    public int Quantity { get; set; }

    public User User { get; init; } = null!;
    public MenuItem MenuItem { get; init; } = null!;
}