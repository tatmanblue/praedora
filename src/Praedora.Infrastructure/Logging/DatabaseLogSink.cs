using System.Text.Json;
using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Logging;

// Implements ILogSink, backing the in-app Log Viewer (design doc §9). Serilog's file sink covers
// process-level logging alongside this.
public class DatabaseLogSink(ILogEntryRepository logEntryRepository) : ILogSink
{
    public async Task WriteAsync(string severity, string component, string message, object? context, CancellationToken ct)
    {
        string? contextJson = context is null ? null : JsonSerializer.Serialize(context, JsonSerializerOptions.Web);

        LogEntry entry = LogEntry.Create(DateTimeOffset.UtcNow, severity, component, message, contextJson);

        await logEntryRepository.AddAsync(entry, ct);
        await logEntryRepository.SaveChangesAsync(ct);
    }
}
