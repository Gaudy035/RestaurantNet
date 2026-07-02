using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

public enum PaymentMethod
{
    Card,
    Cash
}

public enum OrderType
{
    DineIn,
    Pickup,
    Delivery
}

[Table("t_order")]
public class Order
{
    [Key]
    [Column("order_id")]
    public int OrderId { get; set; }

    [Required]
    [Column("location_id")]
    public int LocationId { get; set; }

    [Column("client_id")]
    public int? ClientId { get; set; }

    [Required]
    [Column("order_time")]
    public DateTime OrderTime { get; set; }

    [Required]
    [Column("payment_method")]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    [Column("order_type")]
    public OrderType OrderType {get; set;}

    public Location Location { get; set; } = null!;

    public Client? Client { get; set; }

    public Delivery? Delivery { get; set; }
}