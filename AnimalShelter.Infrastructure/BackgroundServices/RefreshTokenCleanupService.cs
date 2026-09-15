 using AnimalShelter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnimalShelter.Infrastructure.BackgroundServices;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Refresh token cleanup service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var db =
                    scope.ServiceProvider
                        .GetRequiredService<AppDbContext>();

                var expiredTokens =
                    await db.RefreshTokens
                        .Where(rt =>
                            rt.ExpiresAt <= DateTime.UtcNow ||
                            rt.IsRevoked)
                        .ToListAsync(stoppingToken);

                if (expiredTokens.Count > 0)
                {
                    db.RefreshTokens.RemoveRange(expiredTokens);

                    await db.SaveChangesAsync(
                        stoppingToken);

                    _logger.LogInformation(
                        "Removed {Count} expired or revoked refresh tokens.",
                        expiredTokens.Count);
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred during refresh token cleanup.");

                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }
        }

        _logger.LogInformation(
            "Refresh token cleanup service stopped.");
    }
}