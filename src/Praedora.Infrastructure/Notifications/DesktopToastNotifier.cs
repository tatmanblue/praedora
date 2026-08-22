using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Notifications;

// Optional desktop toast notifications (design doc §4 marks this "phase 2" — not part of the
// six-step build sequence). Not wired into DI until it's actually implemented.
public class DesktopToastNotifier : INotifier
{
    public Task NotifyReviewQueueUpdatedAsync(int pendingCount, CancellationToken ct)
    {
        throw new NotImplementedException("Desktop toast notifications are a phase-2 feature, not yet scheduled.");
    }

    public Task NotifyBoardUpdatedAsync(Guid applicationId, CancellationToken ct)
    {
        throw new NotImplementedException("Desktop toast notifications are a phase-2 feature, not yet scheduled.");
    }
}
