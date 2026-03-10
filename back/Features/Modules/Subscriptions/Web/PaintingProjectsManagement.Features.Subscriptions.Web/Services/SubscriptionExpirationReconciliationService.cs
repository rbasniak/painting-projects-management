using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed class SubscriptionExpirationReconciliationService(
    IServiceScopeFactory scopeFactory,
    ILogger<SubscriptionExpirationReconciliationService> logger) : BackgroundService
{
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ReconcileAsync(stoppingToken);

        using var timer = new PeriodicTimer(ExecutionInterval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ReconcileAsync(stoppingToken);
        }
    }

    private async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DbContext>();
            var now = DateTime.UtcNow;

            var expired = await context.Set<TenantSubscription>()
                .Where(x => x.Tier != SubscriptionTier.Free)
                .Where(x => x.CurrentPeriodEndUtc.HasValue && x.CurrentPeriodEndUtc.Value <= now)
                .ToListAsync(cancellationToken);

            foreach (var subscription in expired)
            {
                if (subscription.CancelAtPeriodEnd)
                {
                    subscription.CancelImmediately(subscription.CurrentPeriodEndUtc ?? now);
                }
                else
                {
                    subscription.Expire(subscription.CurrentPeriodEndUtc ?? now);
                }
            }

            if (expired.Count > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Reconciled {Count} expired subscriptions.", expired.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reconcile expired subscriptions.");
        }
    }
}
