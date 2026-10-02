using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;


[Table("t_employee")]
public class Employee
{
    [Key]
    [Column("user_id")]
    public int UserId { get; init; }

    [Column("is_admin")]
    public bool IsAdmin { get; set; }

    public User User { get; init; } = null!;

    public ICollection<LocationEmployee> LocationEmployees { get; init; } = new List<LocationEmployee>();
}