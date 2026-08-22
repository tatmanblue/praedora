using Microsoft.AspNetCore.Components;
using Praedora.Application.DTOs;
using Praedora.Core.Enums;
using Praedora.Web.Services;

namespace Praedora.Web.Components.Pages;

public partial class ReviewQueuePanel : ComponentBase, IAsyncDisposable
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(7);

    [Inject]
    public PraedoraApiClient ApiClient { get; set; } = null!;

    private List<CandidateEventDto> pending = [];
    private List<ApplicationSummaryDto> applications = [];
    private SyncStatusDto? syncStatus;
    private bool isLoading = true;
    private bool isSyncing;
    private Guid? editingId;
    private ApplicationStatus editStatus;
    private Guid? editApplicationId;
    private PeriodicTimer? pollTimer;
    private CancellationTokenSource? pollCts;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
        StartPolling();
    }

    private async Task LoadAsync()
    {
        isLoading = true;
        pending = await ApiClient.GetPendingReviewQueueAsync(CancellationToken.None);
        applications = await ApiClient.GetBoardAsync(CancellationToken.None);
        syncStatus = await ApiClient.GetSyncStatusAsync(CancellationToken.None);
        isLoading = false;
    }

    private bool SyncFailed => syncStatus?.Status?.StartsWith("Error", StringComparison.Ordinal) == true;
    private bool IsSyncing => syncStatus?.IsSyncing == true;

    private void StartPolling()
    {
        pollCts = new CancellationTokenSource();
        pollTimer = new PeriodicTimer(PollInterval);
        _ = PollLoopAsync(pollTimer, pollCts.Token);
    }

    private async Task PollLoopAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                await LoadAsync();
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on dispose.
        }
    }

    private string? CompanyNameFor(Guid? applicationId)
    {
        return applicationId is null
            ? null
            : applications.FirstOrDefault(application => application.Id == applicationId)?.CompanyName;
    }

    private async Task SyncNowAsync()
    {
        isSyncing = true;
        await ApiClient.TriggerSyncAsync(CancellationToken.None);
        await LoadAsync();
        isSyncing = false;
    }

    private async Task ConfirmAsync(Guid id)
    {
        await ApiClient.ConfirmCandidateEventAsync(id, CancellationToken.None);
        await LoadAsync();
    }

    private async Task DismissAsync(Guid id)
    {
        await ApiClient.DismissCandidateEventAsync(id, CancellationToken.None);
        await LoadAsync();
    }

    private void StartEdit(CandidateEventDto candidateEvent)
    {
        editingId = candidateEvent.Id;
        editStatus = candidateEvent.ProposedStatus;
        editApplicationId = candidateEvent.MatchedApplicationId;
    }

    private void CancelEdit()
    {
        editingId = null;
    }

    private async Task SaveEditAsync(Guid id)
    {
        if (editApplicationId is null)
        {
            return;
        }

        await ApiClient.EditCandidateEventAsync(id, new EditCandidateEventRequest(editStatus, editApplicationId.Value), CancellationToken.None);
        editingId = null;
        await LoadAsync();
    }

    public async ValueTask DisposeAsync()
    {
        pollCts?.Cancel();
        pollTimer?.Dispose();
        pollCts?.Dispose();
        await Task.CompletedTask;
    }
}
