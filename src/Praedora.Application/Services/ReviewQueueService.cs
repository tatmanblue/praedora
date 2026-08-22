using Praedora.Core.Enums;

namespace Praedora.Application.Services;

// Confirm / edit / dismiss actions on a CandidateEvent — the only path by which an
// email-detected event becomes a real StatusEvent (design doc §10). Deferred to build
// sequence step 4.
public class ReviewQueueService
{
    public Task ConfirmAsync(Guid candidateEventId, CancellationToken ct)
    {
        throw new NotImplementedException("Review queue arrives in build sequence step 4.");
    }

    public Task DismissAsync(Guid candidateEventId, CancellationToken ct)
    {
        throw new NotImplementedException("Review queue arrives in build sequence step 4.");
    }

    public Task EditAsync(Guid candidateEventId, ApplicationStatus newStatus, Guid matchedApplicationId, CancellationToken ct)
    {
        throw new NotImplementedException("Review queue arrives in build sequence step 4.");
    }
}
