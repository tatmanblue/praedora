using Praedora.Core.Entities;

namespace Praedora.Application.DTOs;

public record LogEntryDto(
    Guid Id,
    DateTimeOffset Timestamp,
    string Severity,
    string Component,
    string Message,
    string? ContextJson);

public static class LogEntryDtoMapping
{
    public static LogEntryDto ToDto(this LogEntry entry)
    {
        return new LogEntryDto(entry.Id, entry.Timestamp, entry.Severity, entry.Component, entry.Message, entry.ContextJson);
    }
}
