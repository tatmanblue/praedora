using Praedora.Core.Entities;

namespace Praedora.Core.Interfaces;

public interface IApplicationMatcher
{
    // Returns a match plus whether a same-company-active conflict exists.
    Task<MatchResult> FindMatchAsync(EmailMessage email, EmailClassification classification, CancellationToken ct);
}

public record MatchResult(Application? Match, bool CompanyConflict, List<Application> Conflicts);
