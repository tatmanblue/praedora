using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Praedora.Application.DTOs;
using Praedora.Core.Enums;
using Praedora.Web.Services;

namespace Praedora.Web.Components.Pages;

public partial class KanbanBoard : ComponentBase, IAsyncDisposable
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(7);

    [Inject]
    public PraedoraApiClient ApiClient { get; set; } = null!;

    private List<ApplicationSummaryDto> applications = [];
    private BoardViewMode viewMode = BoardViewMode.Kanban;
    private bool isLoading = true;
    private bool isSubmitting;
    private bool showCaptureForm;
    private CaptureFormModel captureForm = new();
    private PeriodicTimer? pollTimer;
    private CancellationTokenSource? pollCts;

    protected override async Task OnInitializedAsync()
    {
        viewMode = await ApiClient.GetBoardViewModeAsync(CancellationToken.None);
        await LoadBoardAsync();
        StartPolling();
    }

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
                await LoadBoardAsync();
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on dispose.
        }
    }

    public async ValueTask DisposeAsync()
    {
        pollCts?.Cancel();
        pollTimer?.Dispose();
        pollCts?.Dispose();
        await Task.CompletedTask;
    }

    private async Task LoadBoardAsync()
    {
        isLoading = true;
        applications = await ApiClient.GetBoardAsync(CancellationToken.None);
        isLoading = false;
    }

    private void ToggleCaptureForm()
    {
        showCaptureForm = !showCaptureForm;
        captureForm = new CaptureFormModel();
    }

    private async Task SubmitCaptureAsync()
    {
        isSubmitting = true;

        await ApiClient.CaptureAsync(
            new CaptureApplicationRequest(
                captureForm.CompanyName, captureForm.RoleTitle, captureForm.JobDescriptionRaw,
                string.IsNullOrWhiteSpace(captureForm.SourceUrl) ? null : captureForm.SourceUrl, null),
            CancellationToken.None);

        isSubmitting = false;
        showCaptureForm = false;
        await LoadBoardAsync();
    }

    private async Task OnCardStatusChangedAsync((Guid ApplicationId, ApplicationStatus NewStatus) change)
    {
        await ApiClient.ProgressStatusAsync(change.ApplicationId, new ProgressStatusRequest(change.NewStatus, null), CancellationToken.None);
        await LoadBoardAsync();
    }

    private class CaptureFormModel
    {
        [Required]
        public string CompanyName { get; set; } = "";

        [Required]
        public string RoleTitle { get; set; } = "";

        [Required]
        public string JobDescriptionRaw { get; set; } = "";

        public string? SourceUrl { get; set; }
    }
}
