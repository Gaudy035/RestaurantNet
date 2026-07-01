using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_booking")]
public class Booking
{
    [Key]
    [Column("booking_id")]
    public int BookingId { get; set; }

    [Column("table_id")]
    public int TableId { get; set; }

    [Column("client_id")]
    public int? ClientId { get; set; }

    [Required]
    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Required]
    [Column("duration")]
    public int Duration { get; set; }

    public Table Table { get; set; } = null!;
    public Client? Client { get; set; }
}