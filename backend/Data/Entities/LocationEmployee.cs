using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_location_employee")]
public class LocationEmployee
{
    [Column("location_id")]
    public int LocationId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("is_manager")]
    public bool IsManager { get; set; }

    public Location Location { get; set; } = null!;
    
    public Employee Employee { get; set; } = null!;
}