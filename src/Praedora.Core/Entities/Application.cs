using Praedora.Core.Enums;

namespace Praedora.Core.Entities;

// The single aggregate: capture and pipeline tracking are the same record from the start,
// there is no separate "saved/favorites" tier.
public class Application
{
    public Guid Id { get; private set; }
    public string CompanyName { get; set; } = "";
    public string RoleTitle { get; set; } = "";
    public string? SourceUrl { get; set; }

    public string JobDescriptionRaw { get; set; } = "";
    public string? JobDescriptionHtml { get; set; }
    public string JdHash { get; private set; } = "";
    public JobDescriptionExtract? JdExtract { get; set; }

    public ApplicationStatus Status { get; private set; }
    public DateTimeOffset CapturedAt { get; private set; }
    public DateTimeOffset? AppliedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public DateTimeOffset LastActivityAt { get; private set; }

    public string? ResumeVersionUsed { get; set; }
    public string? Notes { get; set; }

    public bool HasFlaggedDuplicate { get; set; }

    public List<StatusEvent> StatusHistory { get; private set; } = [];
    public List<Contact> Contacts { get; private set; } = [];

    private Application()
    {
    }

    public static Application Create(
        string companyName,
        string roleTitle,
        string jobDescriptionRaw,
        DateTimeOffset occurredAt,
        string? sourceUrl = null,
        string? jobDescriptionHtml = null)
    {
        Application application = new()
        {
            Id = Guid.NewGuid(),
            CompanyName = companyName,
            RoleTitle = roleTitle,
            SourceUrl = sourceUrl,
            JobDescriptionRaw = jobDescriptionRaw,
            JobDescriptionHtml = jobDescriptionHtml,
            JdHash = JobDescriptionHasher.ComputeHash(jobDescriptionRaw),
            CapturedAt = occurredAt,
            LastActivityAt = occurredAt
        };

        application.Apply(StatusEvent.Create(
            application.Id, ApplicationStatus.Captured, occurredAt, EventSource.Manual));

        return application;
    }

    // The ONLY way Status changes. Called exclusively from a confirmed manual action or a
    // confirmed candidate — never from a classifier directly (decision #4).
    public void Apply(StatusEvent evt)
    {
        Status = evt.Status;
        LastActivityAt = evt.OccurredAt;

        if (evt.Status == ApplicationStatus.Applied && AppliedAt is null)
        {
            AppliedAt = evt.OccurredAt;
        }

        if (evt.Status is ApplicationStatus.Rejected or ApplicationStatus.Withdrawn
            or ApplicationStatus.Closed)
        {
            ClosedAt = evt.OccurredAt;
        }

        StatusHistory.Add(evt);
    }
}
