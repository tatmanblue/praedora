using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;

namespace Praedora.Application.Services;

// Confirm / edit / dismiss actions on a CandidateEvent — the only path by which an
// email-detected event becomes a real StatusEvent (design doc §10).
public class ReviewQueueService(ICandidateEventRepository candidateEventRepository, ApplicationService applicationService)
{
    public Task<List<CandidateEvent>> GetPendingAsync(CancellationToken ct)
    {
        return candidateEventRepository.GetPendingAsync(ct);
    }

    public async Task ConfirmAsync(Guid candidateEventId, CancellationToken ct)
    {
        CandidateEvent candidateEvent = await GetOrThrowAsync(candidateEventId, ct);

        if (candidateEvent.MatchedApplicationId is null)
        {
            throw new InvalidOperationException(
                "This candidate has no matched application — edit it to link one before confirming.");
        }

        await applicationService.ApplyEmailConfirmedStatusAsync(
            candidateEvent.MatchedApplicationId.Value, candidateEvent.ProposedStatus,
            candidateEvent.SourceEmailId, candidateEvent.Confidence, candidateEvent.RawSnippet, ct);

        candidateEvent.Confirm();
        await candidateEventRepository.SaveChangesAsync(ct);
    }

    public async Task DismissAsync(Guid candidateEventId, CancellationToken ct)
    {
        CandidateEvent candidateEvent = await GetOrThrowAsync(candidateEventId, ct);

        candidateEvent.Dismiss();
        await candidateEventRepository.SaveChangesAsync(ct);
    }

    // Corrects and commits in one step: applies the corrected status/match to the target
    // Application immediately, rather than staging the correction for a separate confirm.
    public async Task EditAsync(Guid candidateEventId, ApplicationStatus newStatus, Guid matchedApplicationId, CancellationToken ct)
    {
        CandidateEvent candidateEvent = await GetOrThrowAsync(candidateEventId, ct);

        await applicationService.ApplyEmailConfirmedStatusAsync(
            matchedApplicationId, newStatus, candidateEvent.SourceEmailId, candidateEvent.Confidence,
            candidateEvent.RawSnippet, ct);

        candidateEvent.Edit(newStatus, matchedApplicationId);
        await candidateEventRepository.SaveChangesAsync(ct);
    }

    private async Task<CandidateEvent> GetOrThrowAsync(Guid candidateEventId, CancellationToken ct)
    {
        return await candidateEventRepository.GetAsync(candidateEventId, ct)
            ?? throw new InvalidOperationException($"CandidateEvent {candidateEventId} not found.");
    }
}
