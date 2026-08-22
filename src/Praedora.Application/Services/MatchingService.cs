using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Application.Services;

// Resolves a classified email to an Application and flags same-company conflicts (design doc
// §8). Implements IApplicationMatcher directly since matching only needs the repository, not an
// external adapter. Deferred to build sequence step 4.
public class MatchingService : IApplicationMatcher
{
    public Task<MatchResult> FindMatchAsync(EmailMessage email, EmailClassification classification, CancellationToken ct)
    {
        throw new NotImplementedException("Application matching arrives in build sequence step 4.");
    }
}
