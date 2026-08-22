using Praedora.Core.Entities;

namespace Praedora.Core.Interfaces;

public interface ICandidateEventRepository
{
    Task<CandidateEvent?> GetAsync(Guid id, CancellationToken ct);
    Task<List<CandidateEvent>> GetPendingAsync(CancellationToken ct);
    Task AddAsync(CandidateEvent candidateEvent, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
