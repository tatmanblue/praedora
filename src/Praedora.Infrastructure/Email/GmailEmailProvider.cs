using System.Text;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Email;

// Implements IEmailProvider via the Gmail API over OAuth (design doc §7.1). Authorization is
// lazy: the first call (whether from the timer worker or the "Sync now" button — decision #2,
// one code path either way) transparently opens the local browser for consent if there is no
// cached token yet, then caches silently via AppSettingDataStore for every call after that.
public partial class GmailEmailProvider(IConfiguration configuration, IAppSettingRepository appSettingRepository) : IEmailProvider
{
    private const int MaxMessagesPerSync = 100;

    public async Task<IReadOnlyList<EmailMessage>> FetchNewMessagesAsync(DateTimeOffset since, CancellationToken ct)
    {
        GmailService service = await CreateServiceAsync(ct);

        List<EmailMessage> messages = [];
        string? pageToken = null;

        do
        {
            UsersResource.MessagesResource.ListRequest listRequest = service.Users.Messages.List("me");
            listRequest.Q = $"after:{since:yyyy/MM/dd}";
            listRequest.PageToken = pageToken;
            listRequest.MaxResults = 50;

            ListMessagesResponse listResponse = await listRequest.ExecuteAsync(ct);

            foreach (Message summary in listResponse.Messages ?? [])
            {
                if (messages.Count >= MaxMessagesPerSync)
                {
                    break;
                }

                Message full = await service.Users.Messages.Get("me", summary.Id).ExecuteAsync(ct);
                messages.Add(ToEmailMessage(full));
            }

            pageToken = listResponse.NextPageToken;
        }
        while (pageToken is not null && messages.Count < MaxMessagesPerSync);

        return messages;
    }

    private async Task<GmailService> CreateServiceAsync(CancellationToken ct)
    {
        string clientId = configuration["GMAIL_OAUTH_CLIENT_ID"] is { Length: > 0 } id
            ? id
            : throw new InvalidOperationException("GMAIL_OAUTH_CLIENT_ID is not set. Fill it in in .env — see docs/Getting_Started.md.");
        string clientSecret = configuration["GMAIL_OAUTH_CLIENT_SECRET"] is { Length: > 0 } secret
            ? secret
            : throw new InvalidOperationException("GMAIL_OAUTH_CLIENT_SECRET is not set. Fill it in in .env — see docs/Getting_Started.md.");

        UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            [GmailService.Scope.GmailReadonly],
            "praedora-user",
            ct,
            new AppSettingDataStore(appSettingRepository));

        return new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Praedora"
        });
    }

    private static EmailMessage ToEmailMessage(Message message)
    {
        string from = HeaderValue(message, "From") ?? "";
        string subject = HeaderValue(message, "Subject") ?? "";
        DateTimeOffset receivedAt = message.InternalDate is long unixMs
            ? DateTimeOffset.FromUnixTimeMilliseconds(unixMs)
            : DateTimeOffset.UtcNow;

        return EmailMessage.Create(message.Id, from, subject, receivedAt, ExtractBodyText(message.Payload));
    }

    private static string? HeaderValue(Message message, string name)
    {
        return message.Payload?.Headers?
            .FirstOrDefault(header => string.Equals(header.Name, name, StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }

    private static string ExtractBodyText(MessagePart? payload)
    {
        if (payload is null)
        {
            return "";
        }

        string? plainText = FindPartBody(payload, "text/plain");
        if (plainText is not null)
        {
            return plainText;
        }

        string? htmlText = FindPartBody(payload, "text/html");
        return htmlText is not null ? HtmlToText(htmlText) : "";
    }

    private static string? FindPartBody(MessagePart part, string mimeType)
    {
        if (string.Equals(part.MimeType, mimeType, StringComparison.OrdinalIgnoreCase) && part.Body?.Data is not null)
        {
            return DecodeBase64Url(part.Body.Data);
        }

        foreach (MessagePart child in part.Parts ?? [])
        {
            string? found = FindPartBody(child, mimeType);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    private static string DecodeBase64Url(string data)
    {
        string normalized = data.Replace('-', '+').Replace('_', '/');
        normalized = normalized.PadRight(normalized.Length + ((4 - (normalized.Length % 4)) % 4), '=');
        return Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
    }

    private static string HtmlToText(string html)
    {
        string withoutTags = HtmlTagRegex().Replace(html, " ");
        return WhitespaceRegex().Replace(withoutTags, " ").Trim();
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
