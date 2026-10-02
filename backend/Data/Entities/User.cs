using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_user")]
public class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; init; }

    [Required]
    [MaxLength(30)]
    [Column("first_name")]
    public string FirstName { get; init; } = string.Empty;
    
    [Required]
    [MaxLength(30)]
    [Column("last_name")]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    public Client? Client { get; init; }

    public Employee? Employee { get; init; }
}