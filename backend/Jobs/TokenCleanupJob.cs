using backend.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace backend.Jobs;

public class TokenCleanupJob: IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TokenCleanupJob> _logger;

    public TokenCleanupJob(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await TokenCleanupJobCore(dbContext, _logger);
    }

    internal async Task TokenCleanupJobCore(AppDbContext dbContext, ILogger logger)
    {
        logger.LogInformation("[{Time}]: Token cleanup start.", DateTime.Now);

        var dateToRemove = DateTimeOffset.UtcNow.AddDays(-1);

        var tokensToRemove = await dbContext.RefreshTokens
            .Where(rt => rt.ExpiresAt <= dateToRemove || rt.RevokedAt <= dateToRemove)
            .ToListAsync();

        var tokensFound = tokensToRemove.Count;

        if (tokensFound == 0)
        {
            logger.LogInformation("No tokens to delete.");
            return;
        }

        dbContext.RefreshTokens.RemoveRange(tokensToRemove);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Removed {Count} tokens", tokensFound);
    }
}