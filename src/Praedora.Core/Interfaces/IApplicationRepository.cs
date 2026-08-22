using Praedora.Core.Entities;

namespace Praedora.Core.Interfaces;

public interface IApplicationRepository
{
    Task<Application?> GetAsync(Guid id, CancellationToken ct);
    Task<List<Application>> GetActiveByCompanyAsync(string companyName, CancellationToken ct);
    Task<List<Application>> GetBoardAsync(CancellationToken ct);
    Task AddAsync(Application application, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
