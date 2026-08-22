using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Xunit;
using Application = Praedora.Core.Entities.Application;

namespace Praedora.Core.Tests;

public class ApplicationTests
{
    [Fact]
    public void Create_StartsInCapturedStatusWithOneHistoryEntry()
    {
        Application application = Application.Create("Acme Corp", "Backend Engineer", "Do backend things.", DateTimeOffset.UtcNow);

        Assert.Equal(ApplicationStatus.Captured, application.Status);
        Assert.Single(application.StatusHistory);
        Assert.Equal(EventSource.Manual, application.StatusHistory[0].Source);
    }

    [Fact]
    public void Apply_ToApplied_SetsAppliedAtOnce()
    {
        Application application = Application.Create("Acme Corp", "Backend Engineer", "Do backend things.", DateTimeOffset.UtcNow);
        DateTimeOffset appliedAt = DateTimeOffset.UtcNow;

        application.Apply(StatusEvent.Create(application.Id, ApplicationStatus.Applied, appliedAt, EventSource.Manual));

        Assert.Equal(ApplicationStatus.Applied, application.Status);
        Assert.Equal(appliedAt, application.AppliedAt);
    }

    [Fact]
    public void Apply_ToRejected_SetsClosedAt()
    {
        Application application = Application.Create("Acme Corp", "Backend Engineer", "Do backend things.", DateTimeOffset.UtcNow);
        DateTimeOffset rejectedAt = DateTimeOffset.UtcNow;

        application.Apply(StatusEvent.Create(application.Id, ApplicationStatus.Rejected, rejectedAt, EventSource.Manual));

        Assert.Equal(ApplicationStatus.Rejected, application.Status);
        Assert.Equal(rejectedAt, application.ClosedAt);
    }
}
