using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Infrastructure.Services;

public class TokenCleanupService : BackgroundService
{
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly IAdminPromotionTokenRepository _promotionRepo;
    private readonly ILogger<TokenCleanupService> _logger;

    public TokenCleanupService(
        IRefreshTokenRepository refreshRepo,
        IAdminPromotionTokenRepository promotionRepo,
        ILogger<TokenCleanupService> logger
    )
    {
        _refreshRepo = refreshRepo;
        _promotionRepo = promotionRepo;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TokenCleanupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupRefreshTokens();
                await CleanupPromotionTokens();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cleaning expired tokens");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task CleanupRefreshTokens()
    {
        var expired = await _refreshRepo.GetExpiredAsync();
        var revoked = await _refreshRepo.GetRevokedAsync();

        if (expired.Any() || revoked.Any())
        {
            await _refreshRepo.DeleteRangeAsync(expired);
            await _refreshRepo.DeleteRangeAsync(revoked);
            await _refreshRepo.SaveChangesAsync();

            _logger.LogInformation(
                "Cleaned {expired} expired and {revoked} revoked refresh tokens.",
                expired.Count,
                revoked.Count
            );
        }
    }

    private async Task CleanupPromotionTokens()
    {
        var expired = await _promotionRepo.GetExpiredAsync();

        if (expired.Any())
        {
            await _promotionRepo.DeleteRangeAsync(expired);
            await _promotionRepo.SaveChangesAsync();

            _logger.LogInformation(
                "Cleaned {expired} expired admin promotion tokens.",
                expired.Count
            );
        }
    }
}
