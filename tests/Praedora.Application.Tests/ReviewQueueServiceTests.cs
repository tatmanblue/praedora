using Praedora.Application.Services;
using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;
using Xunit;
using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Application.Tests;

public class ReviewQueueServiceTests
{
    [Fact]
    public async Task ConfirmAsync_AppliesStatusEvent_AndMarksConfirmed()
    {
        JobApplication application = JobApplication.Create(
            "Acme Corp", "Backend Engineer", "Do backend things.", DateTimeOffset.UtcNow);
        CandidateEvent candidateEvent = CandidateEvent.Create(
            Guid.NewGuid(), ApplicationStatus.Interviewing, 0.9, DateTimeOffset.UtcNow,
            matchedApplicationId: application.Id, rawSnippet: "Let's schedule a call.");

        FakeApplicationRepository applicationRepository = new([application]);
        FakeCandidateEventRepository candidateEventRepository = new([candidateEvent]);
        ReviewQueueService reviewQueueService = new(
            candidateEventRepository, new ApplicationService(applicationRepository, candidateEventRepository));

        await reviewQueueService.ConfirmAsync(candidateEvent.Id, CancellationToken.None);

        Assert.Equal(ApplicationStatus.Interviewing, application.Status);
        Assert.Equal(EventSource.EmailConfirmed, application.StatusHistory[^1].Source);
        Assert.Equal(CandidateStatus.Confirmed, candidateEvent.Status);
    }

    [Fact]
    public async Task ConfirmAsync_Throws_WhenNoMatchedApplication()
    {
        CandidateEvent candidateEvent = CandidateEvent.Create(
            Guid.NewGuid(), ApplicationStatus.Applied, 0.5, DateTimeOffset.UtcNow, matchedApplicationId: null);

        FakeApplicationRepository applicationRepository = new([]);
        FakeCandidateEventRepository candidateEventRepository = new([candidateEvent]);
        ReviewQueueService reviewQueueService = new(
            candidateEventRepository, new ApplicationService(applicationRepository, candidateEventRepository));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => reviewQueueService.ConfirmAsync(candidateEvent.Id, CancellationToken.None));
    }

    [Fact]
    public async Task DismissAsync_MarksDismissed_AndDoesNotTouchApplication()
    {
        JobApplication application = JobApplication.Create(
            "Acme Corp", "Backend Engineer", "Do backend things.", DateTimeOffset.UtcNow);
        CandidateEvent candidateEvent = CandidateEvent.Create(
            Guid.NewGuid(), ApplicationStatus.Rejected, 0.7, DateTimeOffset.UtcNow, matchedApplicationId: application.Id);

        FakeApplicationRepository applicationRepository = new([application]);
        FakeCandidateEventRepository candidateEventRepository = new([candidateEvent]);
        ReviewQueueService reviewQueueService = new(
            candidateEventRepository, new ApplicationService(applicationRepository, candidateEventRepository));

        await reviewQueueService.DismissAsync(candidateEvent.Id, CancellationToken.None);

        Assert.Equal(CandidateStatus.Dismissed, candidateEvent.Status);
        Assert.Equal(ApplicationStatus.Captured, application.Status);
        Assert.Single(application.StatusHistory);
    }

    [Fact]
    public async Task EditAsync_AppliesCorrectedStatusToTheChosenApplication_AndMarksEdited()
    {
        JobApplication wrongApplication = JobApplication.Create(
            "Acme Corp", "Backend Engineer", "Do backend things.", DateTimeOffset.UtcNow);
        JobApplication correctApplication = JobApplication.Create(
            "Acme Holdings", "Backend Engineer", "Do backend things.", DateTimeOffset.UtcNow);
        CandidateEvent candidateEvent = CandidateEvent.Create(
            Guid.NewGuid(), ApplicationStatus.Applied, 0.4, DateTimeOffset.UtcNow,
            matchedApplicationId: wrongApplication.Id, companyConflictFlagged: true);

        FakeApplicationRepository applicationRepository = new([wrongApplication, correctApplication]);
        FakeCandidateEventRepository candidateEventRepository = new([candidateEvent]);
        ReviewQueueService reviewQueueService = new(
            candidateEventRepository, new ApplicationService(applicationRepository, candidateEventRepository));

        await reviewQueueService.EditAsync(
            candidateEvent.Id, ApplicationStatus.RecruiterScreen, correctApplication.Id, CancellationToken.None);

        Assert.Equal(ApplicationStatus.RecruiterScreen, correctApplication.Status);
        Assert.Equal(ApplicationStatus.Captured, wrongApplication.Status);
        Assert.Equal(CandidateStatus.Edited, candidateEvent.Status);
        Assert.Equal(correctApplication.Id, candidateEvent.MatchedApplicationId);
    }

    private class FakeApplicationRepository(List<JobApplication> applications) : IApplicationRepository
    {
        public Task<JobApplication?> GetAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult(applications.FirstOrDefault(application => application.Id == id));
        }

        public Task<List<JobApplication>> GetActiveByCompanyAsync(string companyName, CancellationToken ct)
        {
            return Task.FromResult(applications.Where(application => application.CompanyName == companyName).ToList());
        }

        public Task<List<JobApplication>> GetBoardAsync(CancellationToken ct)
        {
            return Task.FromResult(applications);
        }

        public Task<List<JobApplication>> GetAllWithDetailsAsync(CancellationToken ct)
        {
            return Task.FromResult(applications);
        }

        public Task<List<Guid>> GetIdsByStatusAsync(ApplicationStatus[]? statuses, CancellationToken ct)
        {
            IEnumerable<JobApplication> query = applications;
            if (statuses is not null)
            {
                query = query.Where(application => statuses.Contains(application.Status));
            }

            return Task.FromResult(query.Select(application => application.Id).ToList());
        }

        public Task AddAsync(JobApplication application, CancellationToken ct)
        {
            applications.Add(application);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            applications.RemoveAll(application => application.Id == id);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            applications.RemoveAll(application => ids.Contains(application.Id));
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }

    private class FakeCandidateEventRepository(List<CandidateEvent> candidateEvents) : ICandidateEventRepository
    {
        public Task<CandidateEvent?> GetAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult(candidateEvents.FirstOrDefault(candidateEvent => candidateEvent.Id == id));
        }

        public Task<List<CandidateEvent>> GetPendingAsync(CancellationToken ct)
        {
            return Task.FromResult(candidateEvents.Where(candidateEvent => candidateEvent.Status == CandidateStatus.Pending).ToList());
        }

        public Task AddAsync(CandidateEvent candidateEvent, CancellationToken ct)
        {
            candidateEvents.Add(candidateEvent);
            return Task.CompletedTask;
        }

        public Task ClearMatchedApplicationForPendingAsync(IReadOnlyCollection<Guid> applicationIds, CancellationToken ct)
        {
            foreach (CandidateEvent candidateEvent in candidateEvents)
            {
                if (candidateEvent.Status == CandidateStatus.Pending
                    && candidateEvent.MatchedApplicationId is { } matchedId
                    && applicationIds.Contains(matchedId))
                {
                    candidateEvent.ClearMatchedApplication();
                }
            }

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
