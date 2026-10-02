using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_refresh_token")]
public class RefreshToken
{
    [Key]
    [Column("token_id")]
    public int TokenId { get; init; }

    [Required]
    [Column("user_id")]
    public int UserId { get; init; }

    [Required]
    [MaxLength(16)]
    [Column("role")]
    public string Role { get; init; } = string.Empty;

    [Required]
    [MaxLength(128)]
    [Column("token_value")]
    public string TokenValue { get; init; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Required]
    [Column("expires_at")]
    public DateTimeOffset ExpiresAt { get; init; }
    
    [Column("revoked_at")]
    public DateTimeOffset? RevokedAt { get; set; }

    [ForeignKey("UserId")]
    public User User { get; init; } = null!;
}