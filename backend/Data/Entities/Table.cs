using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_table")]
public class Table
{
    [Key]
    [Column("table_id")]
    public int TableId { get; init; }

    [Required]
    [Column("location_id")]
    public int LocationId { get; init; }

    [Required]
    [Column("seats")]
    public int Seats { get; init; }

    public Location Location { get; init; } = null!;
    public ICollection<Booking> Bookings { get; init; } = new List<Booking>();
}