using Praedora.Core.Entities;

namespace Praedora.Core.Interfaces;

public interface ILogEntryRepository
{
    Task AddAsync(LogEntry entry, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);

    Task<List<LogEntry>> QueryAsync(
        string? severity, string? component, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct);

    Task ClearAllAsync(CancellationToken ct);
}
