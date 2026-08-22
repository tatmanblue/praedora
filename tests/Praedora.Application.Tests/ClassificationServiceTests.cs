using Praedora.Application.Services;
using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;
using Xunit;
using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Application.Tests;

public class ClassificationServiceTests
{
    private static readonly EmailMessage Email = EmailMessage.Create(
        "msg-1", "recruiter@acme.example", "Update on your application", DateTimeOffset.UtcNow, "body");

    [Fact]
    public async Task ClassifyAsync_ReturnsNull_WhenNotJobRelated()
    {
        EmailClassification classification = new(false, null, null, 0.1, null, null);
        ClassificationService classificationService = new(
            new FakeLlmClassifier(classification), new FakeApplicationMatcher(new MatchResult(null, false, [])));

        CandidateEvent? result = await classificationService.ClassifyAsync(Email, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ClassifyAsync_ReturnsNull_WhenJobRelatedButNoInferredStatus()
    {
        EmailClassification classification = new(true, "Acme Corp", null, 0.5, null, null);
        ClassificationService classificationService = new(
            new FakeLlmClassifier(classification), new FakeApplicationMatcher(new MatchResult(null, false, [])));

        CandidateEvent? result = await classificationService.ClassifyAsync(Email, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ClassifyAsync_ReturnsCandidateEvent_WithMatchedApplication()
    {
        JobApplication application = JobApplication.Create(
            "Acme Corp", "Backend Engineer", "Do backend things.", DateTimeOffset.UtcNow);
        EmailClassification classification = new(
            true, "Acme Corp", ApplicationStatus.Interviewing, 0.87, "Looks like an interview invite.", "Let's schedule a call.");
        ClassificationService classificationService = new(
            new FakeLlmClassifier(classification), new FakeApplicationMatcher(new MatchResult(application, false, [])));

        CandidateEvent? result = await classificationService.ClassifyAsync(Email, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(application.Id, result.MatchedApplicationId);
        Assert.Equal(ApplicationStatus.Interviewing, result.ProposedStatus);
        Assert.Equal(0.87, result.Confidence);
        Assert.False(result.CompanyConflictFlagged);
        Assert.Equal(CandidateStatus.Pending, result.Status);
    }

    [Fact]
    public async Task ClassifyAsync_FlagsConflict_WhenMatcherReportsOne()
    {
        EmailClassification classification = new(true, "Acme Corp", ApplicationStatus.Applied, 0.6, null, null);
        ClassificationService classificationService = new(
            new FakeLlmClassifier(classification), new FakeApplicationMatcher(new MatchResult(null, true, [])));

        CandidateEvent? result = await classificationService.ClassifyAsync(Email, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.MatchedApplicationId);
        Assert.True(result.CompanyConflictFlagged);
    }

    private class FakeLlmClassifier(EmailClassification result) : ILlmClassifier
    {
        public Task<EmailClassification> ClassifyAsync(EmailMessage email, CancellationToken ct)
        {
            return Task.FromResult(result);
        }
    }

    private class FakeApplicationMatcher(MatchResult result) : IApplicationMatcher
    {
        public Task<MatchResult> FindMatchAsync(EmailMessage email, EmailClassification classification, CancellationToken ct)
        {
            return Task.FromResult(result);
        }
    }
}
