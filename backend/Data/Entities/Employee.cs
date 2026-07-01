using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_employee")]
public class Employee
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("is_admin")]
    public bool IsAdmin { get; set; }

    [MaxLength(30)]
    [Column("position")]
    public string Position { get; set; } = string.Empty;

    public User User { get; set; } = null!;

    public ICollection<LocationEmployee> LocationEmployees { get; set; } = new List<LocationEmployee>();
}