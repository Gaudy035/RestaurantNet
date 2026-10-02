using System.Security.Cryptography;
using backend.Data;
using backend.Data.Entities;
using backend.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace backend.Tests.Jobs;

public class TokenCleanupJobTests: IDisposable
{
    private readonly AppDbContext _context;

    public TokenCleanupJobTests()
    {
        // These tests uses EfCore in memory database instead of SQLite, because SQLite is running into an issue with query found in clenaup job that wouldn't occur in postgres prod enviornment
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<Client> SeedClientUser()
    {
        var newClient = new Client
        {
            PhoneNumber = "123 456 789",
            User = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123pass123")
            }
        };

        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();

        return newClient;
    }

    private async Task<RefreshToken> SeedRefreshToken(Client testUser, DateTimeOffset expiration, DateTimeOffset? revocation = null)
    {
        var randomBytes = new Byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        var newTokenValue = Convert.ToBase64String(randomBytes);

        var newRefreshToken = new RefreshToken
        {
            TokenValue = newTokenValue,
            UserId = testUser.UserId,
            Role = "Client",
            ExpiresAt = expiration,
            RevokedAt = revocation,
            IsActive = revocation != null ? false : true,
            User = testUser.User
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return newRefreshToken;
    }

    [Fact]
    public async Task TokenCleanupJob_WhenOldTokensFound_RemovesOldTokens()
    {
        var testUser = await SeedClientUser();
        await SeedRefreshToken(testUser, DateTimeOffset.UtcNow.AddDays(7));
        await SeedRefreshToken(testUser, DateTimeOffset.UtcNow.AddDays(7), DateTimeOffset.UtcNow.AddDays(-1));
        await SeedRefreshToken(testUser, DateTimeOffset.UtcNow.AddDays(-1));

        var tokenCountBefore = await _context.RefreshTokens.CountAsync();

        Assert.Equal(3, tokenCountBefore);

        await TokenCleanupJob.TokenCleanupJobCore(_context, NullLogger.Instance);

        var tokenCountAfter = await _context.RefreshTokens.CountAsync();

        Assert.Equal(1, tokenCountAfter);
    }

    [Fact]
    public async Task TokenCleanupJob_WhenNoTokensFound_DoesntDoAnything()
    {
        var testUser = await SeedClientUser();
        await SeedRefreshToken(testUser, DateTimeOffset.UtcNow.AddDays(2));
        await SeedRefreshToken(testUser, DateTimeOffset.UtcNow.AddDays(3));

        var tokenCountBefore = await _context.RefreshTokens.CountAsync();

        Assert.Equal(2, tokenCountBefore);

        await TokenCleanupJob.TokenCleanupJobCore(_context, NullLogger.Instance);

        var tokenCountAfter = await _context.RefreshTokens.CountAsync();

        Assert.Equal(2, tokenCountAfter);
    }
}