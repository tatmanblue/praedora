using Microsoft.EntityFrameworkCore;
using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Data.Repositories;

public class CandidateEventRepository(PraedoraDbContext dbContext) : ICandidateEventRepository
{
    public Task<CandidateEvent?> GetAsync(Guid id, CancellationToken ct)
    {
        return dbContext.CandidateEvents.FirstOrDefaultAsync(candidateEvent => candidateEvent.Id == id, ct);
    }

    public async Task<List<CandidateEvent>> GetPendingAsync(CancellationToken ct)
    {
        // Sorted client-side: the Sqlite provider can't translate ORDER BY over DateTimeOffset.
        List<CandidateEvent> pending = await dbContext.CandidateEvents
            .Where(candidateEvent => candidateEvent.Status == CandidateStatus.Pending)
            .ToListAsync(ct);
        return [.. pending.OrderByDescending(candidateEvent => candidateEvent.DetectedAt)];
    }

    public Task AddAsync(CandidateEvent candidateEvent, CancellationToken ct)
    {
        return dbContext.CandidateEvents.AddAsync(candidateEvent, ct).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}
