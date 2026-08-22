using Praedora.Application.Services;
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
        ApplicationService applicationService = new(repository);

        JobApplication captured = await applicationService.CaptureAsync(
            "Acme Corp", "Backend Engineer", "Do backend things.", null, null, CancellationToken.None);

        Assert.True(captured.HasFlaggedDuplicate);
    }

    [Fact]
    public async Task CaptureAsync_DoesNotFlagDuplicate_WhenNoActiveApplicationForCompany()
    {
        FakeApplicationRepository repository = new([]);
        ApplicationService applicationService = new(repository);

        JobApplication captured = await applicationService.CaptureAsync(
            "Acme Corp", "Backend Engineer", "Do backend things.", null, null, CancellationToken.None);

        Assert.False(captured.HasFlaggedDuplicate);
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
