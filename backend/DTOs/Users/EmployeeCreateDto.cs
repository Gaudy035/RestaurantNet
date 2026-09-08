using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Users;

public class EmployeeCreateDto
{
    [Required]
    [MaxLength(30)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(30)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
}