using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_location")]
public class Location
{
    [Key]
    [Column("location_id")]
    public int LocationId { get; init; }

    [MaxLength(50)]
    [Column("city")]
    public string City { get; init; } = string.Empty;

    [MaxLength(255)]
    [Column("address")]
    public string Address { get; init; } = string.Empty;

    public ICollection<LocationEmployee> LocationEmployees { get; init; } = new List<LocationEmployee>();

    public ICollection<Table> Tables { get; init; } = new List<Table>();
    
    public ICollection<Order> Orders { get; init; } = new List<Order>();
}