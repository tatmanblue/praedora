using Praedora.Core.Entities;
using Praedora.Core.Enums;

namespace Praedora.Application.DTOs;

// The HTTP boundary between Praedora.Api and Praedora.Web. Application/StatusEvent/Contact use
// factory methods and private setters, which System.Text.Json can't round-trip on the Web side —
// these plain records carry only what the UI needs to render.
public record ApplicationSummaryDto(
    Guid Id,
    string CompanyName,
    string RoleTitle,
    ApplicationStatus Status,
    DateTimeOffset LastActivityAt,
    bool HasFlaggedDuplicate);

public record ApplicationDetailDto(
    Guid Id,
    string CompanyName,
    string RoleTitle,
    string? SourceUrl,
    string JobDescriptionRaw,
    string? JobDescriptionHtml,
    JobDescriptionExtract? JdExtract,
    ApplicationStatus Status,
    DateTimeOffset CapturedAt,
    DateTimeOffset? AppliedAt,
    DateTimeOffset? ClosedAt,
    DateTimeOffset LastActivityAt,
    string? ResumeVersionUsed,
    string? Notes,
    bool HasFlaggedDuplicate,
    List<StatusEventDto> StatusHistory,
    List<ContactDto> Contacts);

public record StatusEventDto(
    Guid Id,
    ApplicationStatus Status,
    DateTimeOffset OccurredAt,
    EventSource Source,
    double? Confidence,
    string? RawSnippet,
    string? Note);

public record ContactDto(Guid Id, string Name, string? Email, string? Role);

public record CaptureApplicationRequest(
    string CompanyName,
    string RoleTitle,
    string JobDescriptionRaw,
    string? SourceUrl,
    string? JobDescriptionHtml);

public record ProgressStatusRequest(ApplicationStatus Status, string? Note);

public record UpdateNotesRequest(string? Notes);

public record DeleteResultDto(int DeletedCount);

public static class ApplicationDtoMapping
{
    public static ApplicationSummaryDto ToSummaryDto(this Core.Entities.Application application)
    {
        return new ApplicationSummaryDto(
            application.Id, application.CompanyName, application.RoleTitle,
            application.Status, application.LastActivityAt, application.HasFlaggedDuplicate);
    }

    public static ApplicationDetailDto ToDetailDto(this Core.Entities.Application application)
    {
        return new ApplicationDetailDto(
            application.Id,
            application.CompanyName,
            application.RoleTitle,
            application.SourceUrl,
            application.JobDescriptionRaw,
            application.JobDescriptionHtml,
            application.JdExtract,
            application.Status,
            application.CapturedAt,
            application.AppliedAt,
            application.ClosedAt,
            application.LastActivityAt,
            application.ResumeVersionUsed,
            application.Notes,
            application.HasFlaggedDuplicate,
            [.. application.StatusHistory
                .OrderBy(statusEvent => statusEvent.OccurredAt)
                .Select(statusEvent => new StatusEventDto(
                    statusEvent.Id, statusEvent.Status, statusEvent.OccurredAt, statusEvent.Source,
                    statusEvent.Confidence, statusEvent.RawSnippet, statusEvent.Note))],
            [.. application.Contacts
                .Select(contact => new ContactDto(contact.Id, contact.Name, contact.Email, contact.Role))]);
    }
}
