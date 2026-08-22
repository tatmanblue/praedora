using Praedora.Core.Entities;

namespace Praedora.Application.Services;

// Will call ILlmClassifier and produce a CandidateEvent for the review queue (design doc §6.3).
// Deferred to build sequence step 4.
public class ClassificationService
{
    public Task<CandidateEvent> ClassifyAsync(EmailMessage email, CancellationToken ct)
    {
        throw new NotImplementedException("Email classification arrives in build sequence step 4.");
    }
}
