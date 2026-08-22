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

    private ApplicationDetailDto? application;
    private bool isLoading = true;

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
}
