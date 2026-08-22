using Praedora.Core.Enums;

namespace Praedora.Core.Entities;

// The staging area between "AI thinks this happened" and "this is now true" (decision #4).
// Confirming one produces a real StatusEvent; nothing here ever mutates Application.Status.
public class CandidateEvent
{
    public Guid Id { get; private set; }
    public Guid? MatchedApplicationId { get; private set; }
    public Guid SourceEmailId { get; private set; }
    public ApplicationStatus ProposedStatus { get; private set; }
    public double Confidence { get; private set; }
    public string? Reasoning { get; private set; }
    public string? RawSnippet { get; private set; }
    public bool CompanyConflictFlagged { get; private set; }
    public CandidateStatus Status { get; private set; }
    public DateTimeOffset DetectedAt { get; private set; }

    private CandidateEvent()
    {
    }

    public static CandidateEvent Create(
        Guid sourceEmailId,
        ApplicationStatus proposedStatus,
        double confidence,
        DateTimeOffset detectedAt,
        Guid? matchedApplicationId = null,
        string? reasoning = null,
        string? rawSnippet = null,
        bool companyConflictFlagged = false)
    {
        return new CandidateEvent
        {
            Id = Guid.NewGuid(),
            SourceEmailId = sourceEmailId,
            ProposedStatus = proposedStatus,
            Confidence = confidence,
            DetectedAt = detectedAt,
            MatchedApplicationId = matchedApplicationId,
            Reasoning = reasoning,
            RawSnippet = rawSnippet,
            CompanyConflictFlagged = companyConflictFlagged,
            Status = CandidateStatus.Pending
        };
    }

    public void Confirm()
    {
        Status = CandidateStatus.Confirmed;
    }

    public void Dismiss()
    {
        Status = CandidateStatus.Dismissed;
    }

    public void Edit(ApplicationStatus proposedStatus, Guid matchedApplicationId)
    {
        ProposedStatus = proposedStatus;
        MatchedApplicationId = matchedApplicationId;
        Status = CandidateStatus.Edited;
    }
}
