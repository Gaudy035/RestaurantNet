using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_order_item")]
public class OrderItem
{
    [Column("order_id")]
    public int OrderId { get; init; }

    [Column("item_id")]
    public int ItemId { get; init; }

    [Required]
    [Column("quantity")]
    public int Quantity { get; init; }

    [Required]
    [Range(typeof(decimal), "0", "10000")]
    [Precision(7, 2)]
    [Column("price")]
    public decimal Price { get; init; }

    public Order Order { get; init; } = null!;

    public MenuItem MenuItem { get; init; } = null!;
}