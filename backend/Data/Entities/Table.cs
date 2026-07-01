using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_table")]
public class Table
{
    [Key]
    [Column("table_id")]
    public int TableId { get; set; }

    [Required]
    [Column("location_id")]
    public int LocationId { get; set; }

    [Required]
    [Column("seats")]
    public int Seats { get; set; }

    public Location Location { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}