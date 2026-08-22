using Microsoft.EntityFrameworkCore;
using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Data.Repositories;

public class LogEntryRepository(PraedoraDbContext dbContext) : ILogEntryRepository
{
    public Task AddAsync(LogEntry entry, CancellationToken ct)
    {
        return dbContext.LogEntries.AddAsync(entry, ct).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<LogEntry>> QueryAsync(
        string? severity, string? component, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct)
    {
        // Filtered/sorted client-side: the Sqlite provider can't translate these predicates over
        // DateTimeOffset (see ApplicationRepository/CandidateEventRepository for the same note).
        List<LogEntry> entries = await dbContext.LogEntries.ToListAsync(ct);

        IEnumerable<LogEntry> filtered = entries;
        if (!string.IsNullOrWhiteSpace(severity))
        {
            filtered = filtered.Where(entry => entry.Severity == severity);
        }

        if (!string.IsNullOrWhiteSpace(component))
        {
            filtered = filtered.Where(entry => entry.Component.Contains(component, StringComparison.OrdinalIgnoreCase));
        }

        if (from is not null)
        {
            filtered = filtered.Where(entry => entry.Timestamp >= from.Value);
        }

        if (to is not null)
        {
            filtered = filtered.Where(entry => entry.Timestamp <= to.Value);
        }

        return [.. filtered.OrderByDescending(entry => entry.Timestamp)];
    }

    public async Task ClearAllAsync(CancellationToken ct)
    {
        await dbContext.LogEntries.ExecuteDeleteAsync(ct);
    }
}
