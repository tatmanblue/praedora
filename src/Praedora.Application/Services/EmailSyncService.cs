using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Application.Services;

// Drives IEmailProvider.FetchNewMessagesAsync, dedupes by Gmail message id, runs a cheap
// deterministic pre-filter before any LLM call, and enqueues surviving messages for
// classification (design doc §7.1). Both the timer worker and the "Sync now" button call this
// same method — there is exactly one sync code path regardless of trigger.
public class EmailSyncService(
    IEmailProvider emailProvider,
    IEmailMessageRepository emailMessageRepository,
    IAppSettingRepository appSettingRepository,
    ClassificationService classificationService,
    ICandidateEventRepository candidateEventRepository,
    ILogSink logSink)
{
    private const string LastSyncedAtKey = "GmailLastSyncedAt";

    // Distinct from LastSyncedAtKey (the fetch watermark, which only advances on a successful
    // Gmail fetch): these two report the outcome of the most recent sync attempt, successful or
    // not, purely for display on the Review Queue page.
    public const string LastCheckedAtKey = "SyncLastCheckedAt";
    public const string LastSyncStatusKey = "SyncLastStatus";

    private static readonly string[] AtsDomains =
    [
        "greenhouse.io", "lever.co", "myworkdayjobs.com", "icims.com", "smartrecruiters.com",
        "bamboohr.com", "ashbyhq.com", "jobvite.com", "taleo.net", "successfactors.com", "workable.com"
    ];

    private static readonly string[] Keywords =
    [
        "application", "applied", "interview", "position", "role", "hiring", "recruit",
        "offer", "unfortunately", "regret", "candidacy", "resume", "cv", "hired", "onboard"
    ];

    public async Task SyncAsync(CancellationToken ct)
    {
        DateTimeOffset since = await GetSinceWatermarkAsync(ct);

        IReadOnlyList<EmailMessage> fetched;
        try
        {
            fetched = await emailProvider.FetchNewMessagesAsync(since, ct);
        }
        catch (Exception ex)
        {
            await logSink.WriteAsync("Error", "EmailSyncService", "Gmail fetch failed.", new { ex.Message }, ct);
            await RecordStatusAsync($"Error: Gmail fetch failed — {ex.Message}", ct);
            return;
        }

        List<EmailMessage> toProcess = [];
        foreach (EmailMessage email in fetched)
        {
            EmailMessage? existing = await emailMessageRepository.GetByExternalIdAsync(email.ExternalMessageId, ct);
            if (existing is null)
            {
                await emailMessageRepository.AddAsync(email, ct);
                await emailMessageRepository.SaveChangesAsync(ct);
                toProcess.Add(email);
            }
            else if (existing.ProcessedAt is null)
            {
                toProcess.Add(existing);
            }
        }

        // Anything left unprocessed from a prior sync (a classification failure, most likely)
        // that this fetch window didn't happen to re-surface — retry it too.
        foreach (EmailMessage stale in await emailMessageRepository.GetUnprocessedAsync(ct))
        {
            if (toProcess.All(email => email.Id != stale.Id))
            {
                toProcess.Add(stale);
            }
        }

        int failureCount = 0;
        foreach (EmailMessage email in toProcess)
        {
            if (!await ProcessOneAsync(email, ct))
            {
                failureCount++;
            }
        }

        await appSettingRepository.SetAsync(LastSyncedAtKey, DateTimeOffset.UtcNow.ToString("O"), ct);

        string status = failureCount == 0
            ? $"Success — checked {toProcess.Count} email(s)"
            : $"Success — checked {toProcess.Count} email(s), {failureCount} classification failure(s) will retry next sync";
        await RecordStatusAsync(status, ct);
    }

    // Returns false if classification failed (left unprocessed for retry) — used only to count
    // failures for the sync status line, never to short-circuit the loop.
    private async Task<bool> ProcessOneAsync(EmailMessage email, CancellationToken ct)
    {
        if (!PassesPreFilter(email))
        {
            email.MarkProcessed(DateTimeOffset.UtcNow, classificationResultJson: null);
            await emailMessageRepository.SaveChangesAsync(ct);
            return true;
        }

        try
        {
            CandidateEvent? candidateEvent = await classificationService.ClassifyAsync(email, ct);
            if (candidateEvent is not null)
            {
                await candidateEventRepository.AddAsync(candidateEvent, ct);
                await candidateEventRepository.SaveChangesAsync(ct);
                await logSink.WriteAsync(
                    "Info", "EmailSyncService",
                    $"Classified email as {candidateEvent.ProposedStatus} (confidence {candidateEvent.Confidence:0.00}).",
                    new { EmailId = email.ExternalMessageId }, ct);
            }

            email.MarkProcessed(DateTimeOffset.UtcNow, classificationResultJson: null);
            await emailMessageRepository.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            await logSink.WriteAsync(
                "Warning", "EmailSyncService",
                $"Classification failed for email {email.ExternalMessageId}; will retry next sync.",
                new { ex.Message }, ct);
            // ProcessedAt stays null — GetUnprocessedAsync picks this back up next sync.
            return false;
        }
    }

    private async Task RecordStatusAsync(string status, CancellationToken ct)
    {
        await appSettingRepository.SetAsync(LastCheckedAtKey, DateTimeOffset.UtcNow.ToString("O"), ct);
        await appSettingRepository.SetAsync(LastSyncStatusKey, status, ct);
    }

    private bool PassesPreFilter(EmailMessage email)
    {
        string fromDomain = email.FromAddress.Contains('@') ? email.FromAddress.Split('@')[^1] : "";
        if (AtsDomains.Any(domain => fromDomain.Contains(domain, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        string haystack = $"{email.Subject} {email.BodyText}";
        return Keywords.Any(keyword => haystack.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<DateTimeOffset> GetSinceWatermarkAsync(CancellationToken ct)
    {
        string? stored = await appSettingRepository.GetAsync(LastSyncedAtKey, ct);
        return stored is not null && DateTimeOffset.TryParse(stored, out DateTimeOffset watermark)
            ? watermark
            : DateTimeOffset.UtcNow.AddDays(-7);
    }
}
