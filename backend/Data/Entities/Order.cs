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
    [Range(0, double.MaxValue)]
    [Column("price")]
    public double Price { get; set; }

    [MaxLength(20)]
    [Column("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [Column("payment_method")]
    public PaymentMethod PaymentMethod { get; set; }

    [Column("is_paid")]
    public bool IsPaid { get; set; }

    [Required]
    [Column("status")]
    public OrderStatus Status { get; set; }

    [Required]
    [Column("order_type")]
    public OrderType OrderType {get; set;}

    public Location Location { get; set; } = null!;

    public Client? Client { get; set; }

    public Delivery? Delivery { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}