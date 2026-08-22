using Praedora.Core.Entities;

namespace Praedora.Core.Interfaces;

public interface IEmailProvider
{
    // Both the timer worker and a "Sync now" button call this same method.
    Task<IReadOnlyList<EmailMessage>> FetchNewMessagesAsync(DateTimeOffset since, CancellationToken ct);
}
