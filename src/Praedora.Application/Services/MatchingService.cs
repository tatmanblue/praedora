using Praedora.Core.Entities;
using Praedora.Core.Interfaces;
using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Application.Services;

// Resolves a classified email to an Application and flags same-company conflicts (design doc
// §8). Implements IApplicationMatcher directly since matching only needs the repository, not an
// external adapter.
public class MatchingService(IApplicationRepository applicationRepository) : IApplicationMatcher
{
    public async Task<MatchResult> FindMatchAsync(EmailMessage email, EmailClassification classification, CancellationToken ct)
    {
        if (classification.InferredCompanyName is null)
        {
            return new MatchResult(null, false, []);
        }

        List<JobApplication> activeMatches = await applicationRepository.GetActiveByCompanyAsync(
            classification.InferredCompanyName, ct);

        return activeMatches.Count switch
        {
            0 => new MatchResult(null, false, []),
            1 => new MatchResult(activeMatches[0], false, []),
            _ => new MatchResult(null, true, activeMatches)
        };
    }
}
