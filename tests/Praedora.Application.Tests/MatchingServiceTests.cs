using Praedora.Application.Services;
using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;
using Xunit;
using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Application.Tests;

public class MatchingServiceTests
{
    private static readonly EmailMessage Email = EmailMessage.Create(
        "msg-1", "recruiter@acme.example", "Update on your application", DateTimeOffset.UtcNow, "body");

    [Fact]
    public async Task FindMatchAsync_ReturnsNoMatch_WhenCompanyNameIsNull()
    {
        MatchingService matchingService = new(new FakeApplicationRepository([]));

        MatchResult result = await matchingService.FindMatchAsync(
            Email, Classification(null), CancellationToken.None);

        Assert.Null(result.Match);
        Assert.False(result.CompanyConflict);
        Assert.Empty(result.Conflicts);
    }

    [Fact]
    public async Task FindMatchAsync_ReturnsNoMatch_WhenNoActiveApplicationForCompany()
    {
        MatchingService matchingService = new(new FakeApplicationRepository([]));

        MatchResult result = await matchingService.FindMatchAsync(
            Email, Classification("Acme Corp"), CancellationToken.None);

        Assert.Null(result.Match);
        Assert.False(result.CompanyConflict);
    }

    [Fact]
    public async Task FindMatchAsync_ReturnsTheMatch_WhenExactlyOneActiveApplicationForCompany()
    {
        JobApplication application = JobApplication.Create(
            "Acme Corp", "Backend Engineer", "Do backend things.", DateTimeOffset.UtcNow);
        MatchingService matchingService = new(new FakeApplicationRepository([application]));

        MatchResult result = await matchingService.FindMatchAsync(
            Email, Classification("Acme Corp"), CancellationToken.None);

        Assert.Equal(application.Id, result.Match?.Id);
        Assert.False(result.CompanyConflict);
    }

    [Fact]
    public async Task FindMatchAsync_FlagsConflict_WhenMultipleActiveApplicationsForCompany()
    {
        JobApplication first = JobApplication.Create(
            "Acme Corp", "Backend Engineer", "Do backend things.", DateTimeOffset.UtcNow);
        JobApplication second = JobApplication.Create(
            "Acme Corp", "Frontend Engineer", "Do frontend things.", DateTimeOffset.UtcNow);
        MatchingService matchingService = new(new FakeApplicationRepository([first, second]));

        MatchResult result = await matchingService.FindMatchAsync(
            Email, Classification("Acme Corp"), CancellationToken.None);

        Assert.Null(result.Match);
        Assert.True(result.CompanyConflict);
        Assert.Equal(2, result.Conflicts.Count);
    }

    private static EmailClassification Classification(string? companyName)
    {
        return new EmailClassification(true, companyName, ApplicationStatus.Applied, 0.9, null, null);
    }

    private class FakeApplicationRepository(List<JobApplication> activeByCompany) : IApplicationRepository
    {
        public Task<JobApplication?> GetAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult(activeByCompany.FirstOrDefault(application => application.Id == id));
        }

        public Task<List<JobApplication>> GetActiveByCompanyAsync(string companyName, CancellationToken ct)
        {
            return Task.FromResult(activeByCompany.Where(application => application.CompanyName == companyName).ToList());
        }

        public Task<List<JobApplication>> GetBoardAsync(CancellationToken ct)
        {
            return Task.FromResult(activeByCompany);
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
            activeByCompany.Add(application);
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
}
