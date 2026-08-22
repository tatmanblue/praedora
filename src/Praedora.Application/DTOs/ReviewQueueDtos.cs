using Praedora.Core.Entities;
using Praedora.Core.Enums;

namespace Praedora.Application.DTOs;

public record CandidateEventDto(
    Guid Id,
    Guid? MatchedApplicationId,
    Guid SourceEmailId,
    ApplicationStatus ProposedStatus,
    double Confidence,
    string? Reasoning,
    string? RawSnippet,
    bool CompanyConflictFlagged,
    CandidateStatus Status,
    DateTimeOffset DetectedAt);

public record EditCandidateEventRequest(ApplicationStatus NewStatus, Guid MatchedApplicationId);

public record SyncStatusDto(DateTimeOffset? LastCheckedAt, string? Status);

public static class CandidateEventDtoMapping
{
    public static CandidateEventDto ToDto(this CandidateEvent candidateEvent)
    {
        return new CandidateEventDto(
            candidateEvent.Id,
            candidateEvent.MatchedApplicationId,
            candidateEvent.SourceEmailId,
            candidateEvent.ProposedStatus,
            candidateEvent.Confidence,
            candidateEvent.Reasoning,
            candidateEvent.RawSnippet,
            candidateEvent.CompanyConflictFlagged,
            candidateEvent.Status,
            candidateEvent.DetectedAt);
    }
}
