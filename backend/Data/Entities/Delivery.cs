using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_delivery")]
public class Delivery
{
    [Key]
    [Column("order_id")]
    public int OrderId { get; init; }

    [Required]
    [MaxLength(50)]
    [Column("city")]
    public string City { get; init; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("address")]
    public string Address { get; init; } = string.Empty;

    public Order Order { get; init; } = null!;
}