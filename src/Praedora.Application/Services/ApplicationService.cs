using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;
using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Application.Services;

// Manual capture, board listing, and manual status progression. Every status change here goes
// through Application.Apply with EventSource.Manual — this is the only path that touches
// Status besides a confirmed CandidateEvent (see ReviewQueueService, step 4).
public class ApplicationService(IApplicationRepository repository, ICandidateEventRepository candidateEventRepository)
{
    private static readonly ApplicationStatus[] TerminalStatuses =
    [
        ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Closed
    ];

    public async Task<JobApplication> CaptureAsync(
        string companyName,
        string roleTitle,
        string jobDescriptionRaw,
        string? sourceUrl,
        string? jobDescriptionHtml,
        CancellationToken ct)
    {
        List<JobApplication> activeDuplicates = await repository.GetActiveByCompanyAsync(companyName, ct);

        JobApplication application = JobApplication.Create(
            companyName, roleTitle, jobDescriptionRaw, DateTimeOffset.UtcNow, sourceUrl, jobDescriptionHtml);

        if (activeDuplicates.Count > 0)
        {
            application.HasFlaggedDuplicate = true;
        }

        await repository.AddAsync(application, ct);
        await repository.SaveChangesAsync(ct);

        return application;
    }

    public Task<List<JobApplication>> GetBoardAsync(CancellationToken ct)
    {
        return repository.GetBoardAsync(ct);
    }

    public Task<JobApplication?> GetAsync(Guid applicationId, CancellationToken ct)
    {
        return repository.GetAsync(applicationId, ct);
    }

    public async Task<JobApplication> ProgressStatusAsync(
        Guid applicationId, ApplicationStatus newStatus, string? note, CancellationToken ct)
    {
        JobApplication application = await repository.GetAsync(applicationId, ct)
            ?? throw new InvalidOperationException($"Application {applicationId} not found.");

        StatusEvent statusEvent = StatusEvent.Create(
            applicationId, newStatus, DateTimeOffset.UtcNow, EventSource.Manual, note);
        application.Apply(statusEvent);

        await repository.SaveChangesAsync(ct);

        return application;
    }

    public async Task<JobApplication> UpdateNotesAsync(Guid applicationId, string? notes, CancellationToken ct)
    {
        JobApplication application = await repository.GetAsync(applicationId, ct)
            ?? throw new InvalidOperationException($"Application {applicationId} not found.");

        application.Notes = notes;

        await repository.SaveChangesAsync(ct);

        return application;
    }

    public async Task<JobApplication> AttachJdExtractAsync(
        Guid applicationId, JobDescriptionExtract jdExtract, CancellationToken ct)
    {
        JobApplication application = await repository.GetAsync(applicationId, ct)
            ?? throw new InvalidOperationException($"Application {applicationId} not found.");

        application.JdExtract = jdExtract;

        await repository.SaveChangesAsync(ct);

        return application;
    }

    // The email-confirmed counterpart to ProgressStatusAsync — used exclusively by
    // ReviewQueueService when a CandidateEvent is confirmed or edited-and-committed. Still the
    // only two paths that ever touch Status, per the class comment above.
    public async Task<JobApplication> ApplyEmailConfirmedStatusAsync(
        Guid applicationId, ApplicationStatus newStatus, Guid sourceEmailId, double confidence, string? rawSnippet,
        CancellationToken ct)
    {
        JobApplication application = await repository.GetAsync(applicationId, ct)
            ?? throw new InvalidOperationException($"Application {applicationId} not found.");

        StatusEvent statusEvent = StatusEvent.Create(
            applicationId, newStatus, DateTimeOffset.UtcNow, EventSource.EmailConfirmed,
            note: null, sourceEmailId: sourceEmailId, confidence: confidence, rawSnippet: rawSnippet);
        application.Apply(statusEvent);

        await repository.SaveChangesAsync(ct);

        return application;
    }

    public async Task DeleteAsync(Guid applicationId, CancellationToken ct)
    {
        await repository.DeleteAsync(applicationId, ct);
        await candidateEventRepository.ClearMatchedApplicationForPendingAsync([applicationId], ct);
        await repository.SaveChangesAsync(ct);
    }

    // scope: only closed/terminal applications (Rejected, Withdrawn, Closed), or every application
    // when onlyClosed is false. Returns the number of applications deleted.
    public async Task<int> DeleteByStatusAsync(bool onlyClosed, CancellationToken ct)
    {
        List<Guid> ids = await repository.GetIdsByStatusAsync(onlyClosed ? TerminalStatuses : null, ct);
        if (ids.Count == 0)
        {
            return 0;
        }

        await repository.DeleteRangeAsync(ids, ct);
        await candidateEventRepository.ClearMatchedApplicationForPendingAsync(ids, ct);
        await repository.SaveChangesAsync(ct);

        return ids.Count;
    }

    public Task<List<JobApplication>> ExportAllAsync(CancellationToken ct)
    {
        return repository.GetAllWithDetailsAsync(ct);
    }
}
