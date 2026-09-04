using Praedora.Core.Entities;
using Praedora.Core.Enums;

namespace Praedora.Core.Interfaces;

public interface IApplicationRepository
{
    Task<Application?> GetAsync(Guid id, CancellationToken ct);
    Task<List<Application>> GetActiveByCompanyAsync(string companyName, CancellationToken ct);
    Task<List<Application>> GetBoardAsync(CancellationToken ct);
    Task<List<Application>> GetAllWithDetailsAsync(CancellationToken ct);
    Task<List<Guid>> GetIdsByStatusAsync(ApplicationStatus[]? statuses, CancellationToken ct);
    Task AddAsync(Application application, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
