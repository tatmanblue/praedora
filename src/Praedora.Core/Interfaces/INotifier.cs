namespace Praedora.Core.Interfaces;

public interface INotifier
{
    Task NotifyReviewQueueUpdatedAsync(int pendingCount, CancellationToken ct);
    Task NotifyBoardUpdatedAsync(Guid applicationId, CancellationToken ct);
}
