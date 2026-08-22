using Microsoft.Extensions.Logging.Abstractions;
using Praedora.Application.Services;
using Praedora.Core.Entities;
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
        ApplicationService applicationService = new(repository);
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
        ApplicationService applicationService = new(repository);
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

        public Task AddAsync(JobApplication application, CancellationToken ct)
        {
            Added.Add(application);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
