using backend.Jobs;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntegrationTests.Jobs;

public class TokenCleanupJobTests: IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public TokenCleanupJobTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task TokenCleanupJobCore_WhenOldTokensFound_RemovesOldTokensAndKeepsRecentOnes()
    {
        var remaining = await DatabaseHelper.ExecuteAsync(_factory, async context =>
        {
            var client = await UserSeeder.SeedClientUser(context);

            await TokenSeeder.SeedRefreshToken(
                context, client.UserId, "active-new",
                expiresAt: DateTimeOffset.UtcNow.AddDays(7));
            await TokenSeeder.SeedRefreshToken(
                context, client.UserId, "revoked-old",
                expiresAt: DateTimeOffset.UtcNow.AddDays(7),
                isActive: false, revokedAt: DateTimeOffset.UtcNow.AddDays(-2));
            await TokenSeeder.SeedRefreshToken(
                context, client.UserId, "expired-old",
                expiresAt: DateTimeOffset.UtcNow.AddDays(-2));
            await TokenSeeder.SeedRefreshToken(
                context, client.UserId, "expired-recent",
                expiresAt: DateTimeOffset.UtcNow.AddHours(-12));

            await TokenCleanupJob.TokenCleanupJobCore(context, NullLogger.Instance, CancellationToken.None);

            return await context.RefreshTokens.AsNoTracking()
                .Select(rt => rt.TokenValue)
                .ToListAsync();
        });

        Assert.Equal(["active-new", "expired-recent"], remaining.OrderBy(v => v));
    }

    [Fact]
    public async Task TokenCleanupJobCore_WhenNoOldTokensFound_DoesNotRemoveAnything()
    {
        var remaining = await DatabaseHelper.ExecuteAsync(_factory, async context =>
        {
            var client = await UserSeeder.SeedClientUser(context);

            await TokenSeeder.SeedRefreshToken(
                context, client.UserId, "active-1",
                expiresAt: DateTimeOffset.UtcNow.AddDays(2));
            await TokenSeeder.SeedRefreshToken(
                context, client.UserId, "active-2",
                expiresAt: DateTimeOffset.UtcNow.AddDays(3));

            await TokenCleanupJob.TokenCleanupJobCore(context, NullLogger.Instance, CancellationToken.None);

            return await context.RefreshTokens.AsNoTracking().CountAsync();
        });

        Assert.Equal(2, remaining);
    }
}
