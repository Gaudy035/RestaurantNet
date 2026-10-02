using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_booking")]
public class Booking
{
    [Key]
    [Column("booking_id")]
    public int BookingId { get; init; }

    [Column("table_id")]
    public int TableId { get; init; }

    [Column("client_id")]
    public int? ClientId { get; init; }

    [Required]
    [Column("start_time")]
    public DateTime StartTime { get; init; }

    [Required]
    [Column("duration")]
    public int Duration { get; init; }

    public Table Table { get; init; } = null!;
    public Client? Client { get; init; }
}