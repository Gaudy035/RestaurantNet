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
    public int LocationId { get; init; }

    [Column("user_id")]
    public int UserId { get; init; }

    [Column("position")]
    public Position Position { get; init; }

    public Location Location { get; init; } = null!;
    
    public Employee Employee { get; init; } = null!;
}