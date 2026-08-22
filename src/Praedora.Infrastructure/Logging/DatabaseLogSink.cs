using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Logging;

// Implements ILogSink, backing the in-app Log Viewer (design doc §9). Serilog's file sink
// covers process-level logging in the meantime. Deferred to build sequence step 5.
public class DatabaseLogSink : ILogSink
{
    public Task WriteAsync(string severity, string component, string message, object? context, CancellationToken ct)
    {
        throw new NotImplementedException("The database-backed Log Viewer arrives in build sequence step 5.");
    }
}
