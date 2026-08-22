using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Praedora.Application.DTOs;
using Praedora.Web.Services;

namespace Praedora.Web.Components.Pages;

public partial class LogViewer : ComponentBase
{
    [Inject]
    public PraedoraApiClient ApiClient { get; set; } = null!;

    [Inject]
    public IJSRuntime JsRuntime { get; set; } = null!;

    private ConfirmDialog confirmDialog = default!;
    private List<LogEntryDto> entries = [];
    private bool isLoading = true;
    private string severityFilter = "";
    private string componentFilter = "";
    private DateTime? fromFilter;
    private DateTime? toFilter;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        isLoading = true;

        DateTimeOffset? from = fromFilter is { } fromDate ? new DateTimeOffset(fromDate, TimeSpan.Zero) : null;
        DateTimeOffset? to = toFilter is { } toDate ? new DateTimeOffset(toDate, TimeSpan.Zero) : null;

        entries = await ApiClient.GetLogsAsync(
            string.IsNullOrWhiteSpace(severityFilter) ? null : severityFilter,
            string.IsNullOrWhiteSpace(componentFilter) ? null : componentFilter,
            from, to, CancellationToken.None);

        isLoading = false;
    }

    private async Task CopyAsTextAsync(LogEntryDto entry)
    {
        string text = $"[{entry.Timestamp:O}] {entry.Severity} {entry.Component}: {entry.Message}"
            + (entry.ContextJson is null ? "" : $"\n{entry.ContextJson}");
        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }

    private async Task ClearAllAsync()
    {
        bool confirmed = await confirmDialog.ShowAsync(
            title: "Delete all log entries?",
            message1: "This will permanently delete every log entry. It can't be undone.",
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

        await ApiClient.ClearLogsAsync(CancellationToken.None);
        await LoadAsync();
    }
}
