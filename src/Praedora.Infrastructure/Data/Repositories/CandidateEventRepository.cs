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

    // Prevents a pending CandidateEvent from pointing at an Application that no longer exists —
    // otherwise ReviewQueueService.ConfirmAsync throws when it tries to apply the status to a
    // deleted Application. Nulling the match degrades it back to "Unmatched" instead of crashing.
    public async Task ClearMatchedApplicationForPendingAsync(IReadOnlyCollection<Guid> applicationIds, CancellationToken ct)
    {
        List<CandidateEvent> affected = await dbContext.CandidateEvents
            .Where(candidateEvent => candidateEvent.Status == CandidateStatus.Pending
                && candidateEvent.MatchedApplicationId != null
                && applicationIds.Contains(candidateEvent.MatchedApplicationId.Value))
            .ToListAsync(ct);

        foreach (CandidateEvent candidateEvent in affected)
        {
            candidateEvent.ClearMatchedApplication();
        }
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}
