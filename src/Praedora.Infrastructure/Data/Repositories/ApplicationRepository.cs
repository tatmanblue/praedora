using Microsoft.EntityFrameworkCore;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;
using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Infrastructure.Data.Repositories;

public class ApplicationRepository(PraedoraDbContext dbContext) : IApplicationRepository
{
    private static readonly ApplicationStatus[] TerminalStatuses =
    [
        ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Closed
    ];

    public Task<JobApplication?> GetAsync(Guid id, CancellationToken ct)
    {
        return dbContext.Applications
            .Include(application => application.StatusHistory)
            .Include(application => application.Contacts)
            .FirstOrDefaultAsync(application => application.Id == id, ct);
    }

    public Task<List<JobApplication>> GetActiveByCompanyAsync(string companyName, CancellationToken ct)
    {
        return dbContext.Applications
            .Where(application => application.CompanyName == companyName
                && !TerminalStatuses.Contains(application.Status))
            .ToListAsync(ct);
    }

    public async Task<List<JobApplication>> GetBoardAsync(CancellationToken ct)
    {
        // Sorted client-side: the Sqlite provider can't translate ORDER BY over DateTimeOffset.
        List<JobApplication> applications = await dbContext.Applications.ToListAsync(ct);
        return [.. applications.OrderByDescending(application => application.LastActivityAt)];
    }

    public Task AddAsync(JobApplication application, CancellationToken ct)
    {
        return dbContext.Applications.AddAsync(application, ct).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}
