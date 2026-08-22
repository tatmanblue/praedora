using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Email;

// Implements IEmailProvider via the Gmail API over OAuth (design doc §7.1). Deferred to build
// sequence step 4 — will read GMAIL_OAUTH_CLIENT_ID/GMAIL_OAUTH_CLIENT_SECRET from .env.
public class GmailEmailProvider : IEmailProvider
{
    public Task<IReadOnlyList<EmailMessage>> FetchNewMessagesAsync(DateTimeOffset since, CancellationToken ct)
    {
        throw new NotImplementedException("Gmail sync arrives in build sequence step 4.");
    }
}
