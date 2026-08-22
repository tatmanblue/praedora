using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Application.Services;

// Calls ILlmClassifier and, for job-related mail, produces a CandidateEvent for the review
// queue (design doc §6.3). Returns null for mail the classifier decides isn't job-related, or
// that doesn't clearly imply one of the tracked statuses — nothing lands in the queue for it.
public class ClassificationService(ILlmClassifier llmClassifier, IApplicationMatcher applicationMatcher)
{
    public async Task<CandidateEvent?> ClassifyAsync(EmailMessage email, CancellationToken ct)
    {
        EmailClassification classification = await llmClassifier.ClassifyAsync(email, ct);

        if (!classification.IsJobRelated || classification.InferredStatus is null)
        {
            return null;
        }

        MatchResult matchResult = await applicationMatcher.FindMatchAsync(email, classification, ct);

        return CandidateEvent.Create(
            email.Id,
            classification.InferredStatus.Value,
            classification.Confidence,
            DateTimeOffset.UtcNow,
            matchedApplicationId: matchResult.Match?.Id,
            reasoning: classification.Reasoning,
            rawSnippet: classification.RawSnippet,
            companyConflictFlagged: matchResult.CompanyConflict);
    }
}
