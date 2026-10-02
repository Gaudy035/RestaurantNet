using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_client")]
public class Client
{
    [Key]   
    [Column("user_id")]
    public int UserId { get; init; }

    [Required]
    [MaxLength(20)]
    [MinLength(9)]
    [Column("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    public User User { get; init; } = null!;

    public ICollection<Booking> Bookings { get; init; } = new List<Booking>();

    public ICollection<Order> Orders { get; init; } = new List<Order>();
}