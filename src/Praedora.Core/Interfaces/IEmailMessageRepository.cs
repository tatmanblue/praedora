using Praedora.Core.Entities;

namespace Praedora.Core.Interfaces;

public interface IEmailMessageRepository
{
    Task<EmailMessage?> GetByExternalIdAsync(string externalMessageId, CancellationToken ct);

    // Emails already stored (dedup succeeded) but never successfully classified — the retry set
    // for the next sync, so a classification failure never permanently strands a message.
    Task<List<EmailMessage>> GetUnprocessedAsync(CancellationToken ct);

    Task AddAsync(EmailMessage email, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
