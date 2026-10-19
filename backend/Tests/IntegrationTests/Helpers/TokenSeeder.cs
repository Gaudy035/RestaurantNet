using backend.Data;
using backend.Data.Entities;

namespace IntegrationTests.Helpers;

public static class TokenSeeder
{
    public static async Task<RefreshToken> SeedRefreshToken(
        AppDbContext context,
        int userId,
        string tokenValue,
        string role = "Client",
        bool isActive = true,
        DateTimeOffset? expiresAt = null,
        DateTimeOffset? revokedAt = null)
    {
        var token = new RefreshToken
        {
            TokenValue = tokenValue,
            UserId = userId,
            Role = role,
            IsActive = isActive,
            ExpiresAt = expiresAt ?? DateTimeOffset.UtcNow.AddDays(7),
            RevokedAt = revokedAt
        };

        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        return token;
    }
}
