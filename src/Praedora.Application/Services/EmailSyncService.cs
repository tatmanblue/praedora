namespace Praedora.Application.Services;

// Will drive IEmailProvider.FetchNewMessagesAsync, dedupe by Gmail message id, and enqueue
// surviving messages for classification (design doc §7.1). Deferred to build sequence step 4.
public class EmailSyncService
{
    public Task SyncAsync(CancellationToken ct)
    {
        throw new NotImplementedException("Gmail sync arrives in build sequence step 4.");
    }
}
