using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

public enum Position
{
    Manager,
    Chef,
    Server,
    Cashier,
    Driver,
}

[Table("t_location_employee")]
public class LocationEmployee
{
    [Column("location_id")]
    public int LocationId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("position")]
    public Position Position { get; set; }

    public Location Location { get; set; } = null!;
    
    public Employee Employee { get; set; } = null!;
}