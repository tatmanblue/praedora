using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Llm;

// Implements IJobDescriptionExtractor via the Claude API. Deferred to build sequence step 2.
public class ClaudeJobDescriptionExtractor : IJobDescriptionExtractor
{
    public Task<JobDescriptionExtract> ExtractAsync(string rawJobDescription, CancellationToken ct)
    {
        throw new NotImplementedException("Job description extraction arrives in build sequence step 2.");
    }
}
