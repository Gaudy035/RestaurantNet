using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Auth;

public class LoginDto
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Password { get; init; } = string.Empty;
}