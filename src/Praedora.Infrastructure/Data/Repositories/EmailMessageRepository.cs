using Microsoft.EntityFrameworkCore;
using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Data.Repositories;

public class EmailMessageRepository(PraedoraDbContext dbContext) : IEmailMessageRepository
{
    public Task<EmailMessage?> GetByExternalIdAsync(string externalMessageId, CancellationToken ct)
    {
        return dbContext.EmailMessages
            .FirstOrDefaultAsync(email => email.ExternalMessageId == externalMessageId, ct);
    }

    public Task<List<EmailMessage>> GetUnprocessedAsync(CancellationToken ct)
    {
        return dbContext.EmailMessages
            .Where(email => email.ProcessedAt == null)
            .ToListAsync(ct);
    }

    public Task AddAsync(EmailMessage email, CancellationToken ct)
    {
        return dbContext.EmailMessages.AddAsync(email, ct).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}
