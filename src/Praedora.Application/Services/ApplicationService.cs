using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;
using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Application.Services;

// Manual capture, board listing, and manual status progression. Every status change here goes
// through Application.Apply with EventSource.Manual — this is the only path that touches
// Status besides a confirmed CandidateEvent (see ReviewQueueService, step 4).
public class ApplicationService(IApplicationRepository repository)
{
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
}
