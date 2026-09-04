using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Praedora.Application.DTOs;
using Praedora.Core.Enums;
using Praedora.Web.Services;

namespace Praedora.Web.Components.Pages;

public partial class ApplicationDetail : ComponentBase
{
    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    public PraedoraApiClient ApiClient { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    private ApplicationDetailDto? application;
    private bool isLoading = true;
    private ConfirmDialog confirmDialog = default!;

    protected override async Task OnParametersSetAsync()
    {
        isLoading = true;
        application = await ApiClient.GetApplicationAsync(Id, CancellationToken.None);
        isLoading = false;
    }

    private async Task OnStatusSelectedAsync(ChangeEventArgs args)
    {
        ApplicationStatus newStatus = Enum.Parse<ApplicationStatus>((string)args.Value!);
        application = await ApiClient.ProgressStatusAsync(Id, new ProgressStatusRequest(newStatus, null), CancellationToken.None);
    }

    private async Task OnNotesChanged(ChangeEventArgs args)
    {
        string? notes = (string?)args.Value;
        application = await ApiClient.UpdateNotesAsync(Id, new UpdateNotesRequest(notes), CancellationToken.None);
    }

    private async Task DeleteAsync()
    {
        bool confirmed = await confirmDialog.ShowAsync(
            title: $"Delete {application!.CompanyName}?",
            message1: $"This will permanently delete \"{application.RoleTitle}\" and its full history. It can't be undone.",
            confirmDialogOptions: new ConfirmDialogOptions
            {
                YesButtonText = "Delete",
                YesButtonColor = ButtonColor.Danger,
                NoButtonText = "Cancel",
                NoButtonColor = ButtonColor.Secondary
            });

        if (!confirmed)
        {
            return;
        }

        await ApiClient.DeleteApplicationAsync(Id, CancellationToken.None);
        NavigationManager.NavigateTo("/");
    }
}
