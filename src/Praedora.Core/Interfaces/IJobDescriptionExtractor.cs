using Praedora.Core.Entities;

namespace Praedora.Core.Interfaces;

public interface IJobDescriptionExtractor
{
    Task<JobDescriptionExtract> ExtractAsync(string rawJobDescription, CancellationToken ct);
}
