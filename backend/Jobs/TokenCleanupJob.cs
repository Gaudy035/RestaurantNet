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
        _logger.LogInformation("[{Time}]: Token cleanup start.", DateTime.Now);

        var dateToRemove = DateTimeOffset.UtcNow.AddDays(-1);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tokensToRemove = await dbContext.RefreshTokens
            .Where(rt => rt.ExpiresAt <= dateToRemove || rt.RevokedAt <= dateToRemove)
            .ToListAsync();

        var tokensFound = tokensToRemove.Count;

        if (tokensFound == 0)
        {
            _logger.LogInformation("No tokens to delete.");
            return;
        }

        dbContext.RefreshTokens.RemoveRange(tokensToRemove);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Removed {Count} tokens", tokensFound);
    }
}