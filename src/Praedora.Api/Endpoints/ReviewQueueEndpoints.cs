using Praedora.Application;
using Praedora.Application.DTOs;
using Praedora.Application.Services;
using Praedora.Core.Entities;
using Praedora.Core.Interfaces;
using Serilog;

namespace Praedora.Api.Endpoints;

// Confirm / edit / dismiss actions on CandidateEvents, plus manual sync (design doc §10, §7.1).
public static class ReviewQueueEndpoints
{
    public static void MapReviewQueueEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup(ApiRoutes.ReviewQueueBase);

        group.MapGet("/", async (ReviewQueueService reviewQueueService, CancellationToken ct) =>
        {
            List<CandidateEvent> pending = await reviewQueueService.GetPendingAsync(ct);
            return Results.Ok(pending.Select(candidateEvent => candidateEvent.ToDto()));
        });

        group.MapGet("/sync-status",
            async (IAppSettingRepository appSettingRepository, SyncActivityTracker syncActivityTracker, CancellationToken ct) =>
            {
                string? checkedAtRaw = await appSettingRepository.GetAsync(EmailSyncService.LastCheckedAtKey, ct);
                string? status = await appSettingRepository.GetAsync(EmailSyncService.LastSyncStatusKey, ct);
                DateTimeOffset? checkedAt = checkedAtRaw is not null && DateTimeOffset.TryParse(checkedAtRaw, out DateTimeOffset parsed)
                    ? parsed
                    : null;

                return Results.Ok(new SyncStatusDto(checkedAt, status, syncActivityTracker.IsSyncing));
            });

        group.MapPost("/{id:guid}/confirm", async (Guid id, ReviewQueueService reviewQueueService, CancellationToken ct) =>
        {
            await reviewQueueService.ConfirmAsync(id, ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/dismiss", async (Guid id, ReviewQueueService reviewQueueService, CancellationToken ct) =>
        {
            await reviewQueueService.DismissAsync(id, ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/edit",
            async (Guid id, EditCandidateEventRequest request, ReviewQueueService reviewQueueService, CancellationToken ct) =>
            {
                await reviewQueueService.EditAsync(id, request.NewStatus, request.MatchedApplicationId, ct);
                return Results.NoContent();
            });

        // Fire-and-forget, deliberately not tied to this request's CancellationToken: the Gmail
        // OAuth flow this can trigger on first use is interactive (opens a browser, waits on a
        // human to click through consent) and can run far longer than any reasonable HTTP
        // timeout. Blocking the request on it means the client's HttpClient times out first,
        // which cancels the request's token, which tears down the local OAuth redirect listener
        // out from under the still-in-progress browser flow. The review queue page already polls
        // every few seconds, so results show up whenever the sync (and any one-time auth) finish.
        group.MapPost("/sync", (IServiceScopeFactory scopeFactory) =>
        {
            _ = Task.Run(async () =>
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                try
                {
                    EmailSyncService emailSyncService = scope.ServiceProvider.GetRequiredService<EmailSyncService>();
                    await emailSyncService.SyncAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Manually-triggered sync failed unexpectedly.");
                }
            });

            return Results.Accepted();
        });
    }
}
