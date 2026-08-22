namespace Praedora.Api.Workers;

// Timer-based Gmail polling (design doc §7.1). Deferred to build sequence step 4 — this loop
// intentionally does nothing yet so the AppHost topology (worker included in the Api host) is
// already correct once EmailSyncService is implemented.
public class EmailSyncWorker(ILogger<EmailSyncWorker> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EmailSyncWorker started; Gmail sync is not yet implemented (build sequence step 4).");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(PollInterval, stoppingToken);
        }
    }
}
