using Microsoft.Extensions.Logging.Abstractions;
using Praedora.Application.Services;
using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;
using Xunit;
using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Application.Tests;

public class CaptureServiceTests
{
    [Fact]
    public async Task CaptureFromExtensionAsync_PopulatesJdExtract_WhenExtractionSucceeds()
    {
        FakeApplicationRepository repository = new([]);
        ApplicationService applicationService = new(repository, new FakeCandidateEventRepository());
        JobDescriptionExtract expectedExtract = new(["C#"], [], null, null, null, null);
        CaptureService captureService = new(
            applicationService, new FakeJobDescriptionExtractor(expectedExtract), NullLogger<CaptureService>.Instance);

        JobApplication captured = await captureService.CaptureFromExtensionAsync(
            "Acme Corp", "Backend Engineer", "Do backend things.", "https://example.com/job/1", CancellationToken.None);

        Assert.Equal(expectedExtract, captured.JdExtract);
    }

    [Fact]
    public async Task CaptureFromExtensionAsync_StillReturnsCapture_WhenExtractionThrows()
    {
        FakeApplicationRepository repository = new([]);
        ApplicationService applicationService = new(repository, new FakeCandidateEventRepository());
        CaptureService captureService = new(
            applicationService, new ThrowingJobDescriptionExtractor(), NullLogger<CaptureService>.Instance);

        JobApplication captured = await captureService.CaptureFromExtensionAsync(
            "Acme Corp", "Backend Engineer", "Do backend things.", "https://example.com/job/1", CancellationToken.None);

        Assert.Null(captured.JdExtract);
        Assert.Contains(repository.Added, application => application.Id == captured.Id);
    }

    private class FakeJobDescriptionExtractor(JobDescriptionExtract result) : IJobDescriptionExtractor
    {
        public Task<JobDescriptionExtract> ExtractAsync(string rawJobDescription, CancellationToken ct)
        {
            return Task.FromResult(result);
        }
    }

    private class ThrowingJobDescriptionExtractor : IJobDescriptionExtractor
    {
        public Task<JobDescriptionExtract> ExtractAsync(string rawJobDescription, CancellationToken ct)
        {
            throw new InvalidOperationException("CLAUDE_API_KEY is not set.");
        }
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
            return Task.FromResult(Added);
        }

        public Task<List<Guid>> GetIdsByStatusAsync(ApplicationStatus[]? statuses, CancellationToken ct)
        {
            IEnumerable<JobApplication> query = Added;
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
            Added.RemoveAll(application => application.Id == id);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            Added.RemoveAll(application => ids.Contains(application.Id));
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
