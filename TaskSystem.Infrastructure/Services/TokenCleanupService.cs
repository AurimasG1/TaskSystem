using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Infrastructure.Services;

public sealed class TokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TokenCleanupService> _logger;

    public TokenCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<TokenCleanupService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TokenCleanupService started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunCleanupCycleAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normalus aplikacijos sustabdymas.
            _logger.LogInformation("TokenCleanupService stopped.");
        }
    }

    private async Task RunCleanupCycleAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var refreshRepo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

            var promotionRepo =
                scope.ServiceProvider.GetRequiredService<IAdminPromotionTokenRepository>();

            await CleanupRefreshTokensAsync(refreshRepo);
            await CleanupPromotionTokensAsync(promotionRepo);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while cleaning expired tokens.");
        }
    }

    private async Task CleanupRefreshTokensAsync(IRefreshTokenRepository refreshRepo)
    {
        var expired = await refreshRepo.GetExpiredAsync();
        var revoked = await refreshRepo.GetRevokedAsync();

        var expiredCount = expired.Count;
        var revokedCount = revoked.Count;

        if (expiredCount == 0 && revokedCount == 0)
        {
            return;
        }

        if (expiredCount > 0)
        {
            await refreshRepo.DeleteRangeAsync(expired);
        }

        if (revokedCount > 0)
        {
            await refreshRepo.DeleteRangeAsync(revoked);
        }

        await refreshRepo.SaveChangesAsync();

        _logger.LogInformation(
            "Cleaned {ExpiredCount} expired and {RevokedCount} revoked refresh tokens.",
            expiredCount,
            revokedCount
        );
    }

    private async Task CleanupPromotionTokensAsync(IAdminPromotionTokenRepository promotionRepo)
    {
        var expired = await promotionRepo.GetExpiredAsync();
        var expiredCount = expired.Count;

        if (expiredCount == 0)
        {
            return;
        }

        await promotionRepo.DeleteRangeAsync(expired);
        await promotionRepo.SaveChangesAsync();

        _logger.LogInformation(
            "Cleaned {ExpiredCount} expired admin promotion tokens.",
            expiredCount
        );
    }
}
