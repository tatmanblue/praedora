namespace Praedora.Core.Interfaces;

public interface ILogSink
{
    Task WriteAsync(string severity, string component, string message, object? context, CancellationToken ct);
}
