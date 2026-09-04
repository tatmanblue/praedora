using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Praedora.Application.DTOs;
using Praedora.Core.Enums;
using Praedora.Web.Services;

namespace Praedora.Web.Components.Pages;

public partial class Admin : ComponentBase
{
    private static readonly ApplicationStatus[] TerminalStatuses =
    [
        ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Closed
    ];

    [Inject]
    public PraedoraApiClient ApiClient { get; set; } = null!;

    private BoardViewMode viewMode;
    private bool isLoading = true;
    private bool showSaved;
    private int totalCount;
    private int closedCount;
    private string? deleteMessage;
    private ConfirmDialog confirmDialog = default!;

    protected override async Task OnInitializedAsync()
    {
        viewMode = await ApiClient.GetBoardViewModeAsync(CancellationToken.None);
        await LoadCountsAsync();
        isLoading = false;
    }

    private async Task LoadCountsAsync()
    {
        List<ApplicationSummaryDto> board = await ApiClient.GetBoardAsync(CancellationToken.None);
        totalCount = board.Count;
        closedCount = board.Count(application => TerminalStatuses.Contains(application.Status));
    }

    private async Task OnViewModeChangedAsync(BoardViewMode newMode)
    {
        viewMode = newMode;
        await ApiClient.SetBoardViewModeAsync(newMode, CancellationToken.None);

        showSaved = true;
        StateHasChanged();
        await Task.Delay(1500);
        showSaved = false;
    }

    private async Task DeleteClosedAsync()
    {
        bool confirmed = await confirmDialog.ShowAsync(
            title: "Delete closed applications?",
            message1: $"This will permanently delete {closedCount} closed application(s) (Rejected, Withdrawn, Closed) and their history. It can't be undone.",
            confirmDialogOptions: new ConfirmDialogOptions
            {
                YesButtonText = "Delete closed",
                YesButtonColor = ButtonColor.Danger,
                NoButtonText = "Cancel",
                NoButtonColor = ButtonColor.Secondary
            });

        if (!confirmed)
        {
            return;
        }

        int deletedCount = await ApiClient.DeleteApplicationsAsync("closed", CancellationToken.None);
        deleteMessage = $"Deleted {deletedCount} closed application(s).";
        await LoadCountsAsync();
    }

    private async Task DeleteAllAsync()
    {
        bool confirmed = await confirmDialog.ShowAsync(
            title: "Delete ALL applications?",
            message1: $"This will permanently delete all {totalCount} application(s), including ones still in progress, and their full history. It can't be undone.",
            confirmDialogOptions: new ConfirmDialogOptions
            {
                YesButtonText = "Delete all",
                YesButtonColor = ButtonColor.Danger,
                NoButtonText = "Cancel",
                NoButtonColor = ButtonColor.Secondary
            });

        if (!confirmed)
        {
            return;
        }

        int deletedCount = await ApiClient.DeleteApplicationsAsync("all", CancellationToken.None);
        deleteMessage = $"Deleted {deletedCount} application(s).";
        await LoadCountsAsync();
    }
}
