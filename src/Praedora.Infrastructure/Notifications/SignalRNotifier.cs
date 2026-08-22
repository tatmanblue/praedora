using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Notifications;

// Implements INotifier by pushing to PraedoraHub (design doc §10). Deferred to build
// sequence step 5.
public class SignalRNotifier : INotifier
{
    public Task NotifyReviewQueueUpdatedAsync(int pendingCount, CancellationToken ct)
    {
        throw new NotImplementedException("Live board/queue updates arrive in build sequence step 5.");
    }

    public Task NotifyBoardUpdatedAsync(Guid applicationId, CancellationToken ct)
    {
        throw new NotImplementedException("Live board/queue updates arrive in build sequence step 5.");
    }
}
