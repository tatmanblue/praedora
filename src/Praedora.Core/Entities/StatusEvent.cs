using Praedora.Core.Enums;

namespace Praedora.Core.Entities;

// Append-only. Current Application.Status is always derivable from the latest event here.
public class StatusEvent
{
    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public ApplicationStatus Status { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public EventSource Source { get; private set; }
    public Guid? SourceEmailId { get; private set; }
    public double? Confidence { get; private set; }
    public string? RawSnippet { get; private set; }
    public string? Note { get; private set; }

    private StatusEvent()
    {
    }

    public static StatusEvent Create(
        Guid applicationId,
        ApplicationStatus status,
        DateTimeOffset occurredAt,
        EventSource source,
        string? note = null,
        Guid? sourceEmailId = null,
        double? confidence = null,
        string? rawSnippet = null)
    {
        return new StatusEvent
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Status = status,
            OccurredAt = occurredAt,
            Source = source,
            Note = note,
            SourceEmailId = sourceEmailId,
            Confidence = confidence,
            RawSnippet = rawSnippet
        };
    }
}
