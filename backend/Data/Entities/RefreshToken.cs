using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_refresh_token")]
public class RefreshToken
{
    [Key]
    [Column("token_id")]
    public int TokenId { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(128)]
    [Column("token_value")]
    public string TokenValue { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Required]
    [Column("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }
    
    [Column("revoked_at")]
    public DateTimeOffset? RevokedAt { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}