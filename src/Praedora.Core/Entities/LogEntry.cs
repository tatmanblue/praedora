namespace Praedora.Core.Entities;

public class LogEntry
{
    public Guid Id { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string Severity { get; private set; } = "";
    public string Component { get; private set; } = "";
    public string Message { get; private set; } = "";
    public string? ContextJson { get; private set; }

    private LogEntry()
    {
    }

    public static LogEntry Create(
        DateTimeOffset timestamp, string severity, string component, string message, string? contextJson = null)
    {
        return new LogEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = timestamp,
            Severity = severity,
            Component = component,
            Message = message,
            ContextJson = contextJson
        };
    }
}
