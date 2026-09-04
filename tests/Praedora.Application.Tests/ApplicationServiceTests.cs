using Praedora.Application.Services;
using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;
using Xunit;
using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Application.Tests;

public class ApplicationServiceTests
{
    [Fact]
    public async Task CaptureAsync_FlagsDuplicate_WhenActiveApplicationExistsForCompany()
    {
        JobApplication existing = JobApplication.Create(
            "Acme Corp", "Frontend Engineer", "Do frontend things.", DateTimeOffset.UtcNow);
        FakeApplicationRepository repository = new([existing]);
        ApplicationService applicationService = new(repository, new FakeCandidateEventRepository());

        JobApplication captured = await applicationService.CaptureAsync(
            "Acme Corp", "Backend Engineer", "Do backend things.", null, null, CancellationToken.None);

        Assert.True(captured.HasFlaggedDuplicate);
    }

    [Fact]
    public async Task CaptureAsync_DoesNotFlagDuplicate_WhenNoActiveApplicationForCompany()
    {
        FakeApplicationRepository repository = new([]);
        ApplicationService applicationService = new(repository, new FakeCandidateEventRepository());

        JobApplication captured = await applicationService.CaptureAsync(
            "Acme Corp", "Backend Engineer", "Do backend things.", null, null, CancellationToken.None);

        Assert.False(captured.HasFlaggedDuplicate);
    }

    [Fact]
    public async Task DeleteByStatusAsync_OnlyClosed_DeletesJustTerminalStatuses()
    {
        JobApplication active = JobApplication.Create(
            "Acme Corp", "Backend Engineer", "Do backend things.", DateTimeOffset.UtcNow);
        JobApplication rejected = JobApplication.Create(
            "Widgets Inc", "QA Engineer", "Test widgets.", DateTimeOffset.UtcNow);
        rejected.Apply(StatusEvent.Create(rejected.Id, ApplicationStatus.Rejected, DateTimeOffset.UtcNow, EventSource.Manual));

        List<JobApplication> applications = [active, rejected];
        FakeApplicationRepository repository = new(applications);
        ApplicationService applicationService = new(repository, new FakeCandidateEventRepository());

        int deletedCount = await applicationService.DeleteByStatusAsync(onlyClosed: true, CancellationToken.None);

        Assert.Equal(1, deletedCount);
        Assert.DoesNotContain(applications, application => application.Id == rejected.Id);
        Assert.Contains(applications, application => application.Id == active.Id);
    }

    private class FakeApplicationRepository(List<JobApplication> activeByCompany) : IApplicationRepository
    {
        public List<JobApplication> Added { get; } = [];

        public Task<JobApplication?> GetAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult(Added.FirstOrDefault(application => application.Id == id));
        }

        public Task<List<JobApplication>> GetActiveByCompanyAsync(string companyName, CancellationToken ct)
        {
            return Task.FromResult(activeByCompany.Where(application => application.CompanyName == companyName).ToList());
        }

        public Task<List<JobApplication>> GetBoardAsync(CancellationToken ct)
        {
            return Task.FromResult(Added);
        }

        public Task<List<JobApplication>> GetAllWithDetailsAsync(CancellationToken ct)
        {
            return Task.FromResult(activeByCompany);
        }

        public Task<List<Guid>> GetIdsByStatusAsync(ApplicationStatus[]? statuses, CancellationToken ct)
        {
            IEnumerable<JobApplication> query = activeByCompany;
            if (statuses is not null)
            {
                query = query.Where(application => statuses.Contains(application.Status));
            }

            return Task.FromResult(query.Select(application => application.Id).ToList());
        }

        public Task AddAsync(JobApplication application, CancellationToken ct)
        {
            Added.Add(application);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            activeByCompany.RemoveAll(application => application.Id == id);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            activeByCompany.RemoveAll(application => ids.Contains(application.Id));
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }

    private class FakeCandidateEventRepository : ICandidateEventRepository
    {
        public Task<CandidateEvent?> GetAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult<CandidateEvent?>(null);
        }

        public Task<List<CandidateEvent>> GetPendingAsync(CancellationToken ct)
        {
            return Task.FromResult(new List<CandidateEvent>());
        }

        public Task AddAsync(CandidateEvent candidateEvent, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task ClearMatchedApplicationForPendingAsync(IReadOnlyCollection<Guid> applicationIds, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
