using Praedora.Application.Services;
using Praedora.Core.Interfaces;

namespace Praedora.Api.Workers;

// Timer-based Gmail polling (design doc §7.1). Runs the identical EmailSyncService.SyncAsync
// that the review queue's "Sync now" button calls — one sync code path regardless of trigger.
public class EmailSyncWorker(IServiceScopeFactory scopeFactory, ILogger<EmailSyncWorker> logger) : BackgroundService
{
    private const string IntervalSettingKey = "EmailSyncIntervalMinutes";
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMinutes(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EmailSyncWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan interval = await GetIntervalAsync(stoppingToken);

            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                EmailSyncService emailSyncService = scope.ServiceProvider.GetRequiredService<EmailSyncService>();
                await emailSyncService.SyncAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // EmailSyncService already logs granular failures via ILogSink; this catch only
                // exists so a truly unexpected exception can't kill the background loop itself.
                logger.LogError(ex, "EmailSyncWorker tick failed unexpectedly.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task<TimeSpan> GetIntervalAsync(CancellationToken ct)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        IAppSettingRepository appSettingRepository = scope.ServiceProvider.GetRequiredService<IAppSettingRepository>();

        string? stored = await appSettingRepository.GetAsync(IntervalSettingKey, ct);
        return stored is not null && int.TryParse(stored, out int minutes) && minutes > 0
            ? TimeSpan.FromMinutes(minutes)
            : DefaultInterval;
    }
}
