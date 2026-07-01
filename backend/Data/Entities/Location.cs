using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_location")]
public class Location
{
    [Key]
    [Column("location_id")]
    public int LocationId { get; set; }

    [MaxLength(50)]
    [Column("city")]
    public string City { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("address")]
    public string Address { get; set; } = string.Empty;

    public ICollection<LocationEmployee> LocationEmployees { get; set; } = new List<LocationEmployee>();
}