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

public enum OrderStatus
{
    Pending,
    Preparing,
    Ready,
    Shipped,
    Completed,
    Cancelled
}

[Table("t_order")]
public class Order
{
    [Key]
    [Column("order_id")]
    public int OrderId { get; init; }

    [Required]
    [Column("location_id")]
    public int LocationId { get; init; }

    [Column("client_id")]
    public int? ClientId { get; init; }

    [Required]
    [Column("order_time")]
    public DateTime OrderTime { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    [Column("price")]
    public double Price { get; init; }

    [MaxLength(20)]
    [Column("phone_number")]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required]
    [Column("payment_method")]
    public PaymentMethod PaymentMethod { get; init; }

    [Column("is_paid")]
    public bool IsPaid { get; set; }

    [Required]
    [Column("status")]
    public OrderStatus Status { get; set; }

    [Required]
    [Column("order_type")]
    public OrderType OrderType {get; init;}

    public Location Location { get; init; } = null!;

    public Client? Client { get; init; }

    public Delivery? Delivery { get; init; }

    public ICollection<OrderItem> OrderItems { get; init; } = new List<OrderItem>();
}