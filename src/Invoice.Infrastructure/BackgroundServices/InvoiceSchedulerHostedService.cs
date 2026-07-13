using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Invoice.Infrastructure.BackgroundServices;

/// <summary>
/// Hourly background loop: generates invoices from due recurring templates and marks
/// unpaid past-due invoices as Overdue. Runs once immediately at startup (after
/// migrations — hosted services start after Program.cs's InitialiseDatabaseAsync).
/// </summary>
public class InvoiceSchedulerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<InvoiceSchedulerHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnceAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var scheduler = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
            await scheduler.ProcessAsync(DateTimeOffset.UtcNow, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invoice scheduler run failed");
        }
    }
}
