using Microsoft.AspNetCore.Components;
using Praedora.Core.Enums;
using Praedora.Web.Services;

namespace Praedora.Web.Components.Pages;

public partial class Admin : ComponentBase
{
    [Inject]
    public PraedoraApiClient ApiClient { get; set; } = null!;

    private BoardViewMode viewMode;
    private bool isLoading = true;
    private bool showSaved;

    protected override async Task OnInitializedAsync()
    {
        viewMode = await ApiClient.GetBoardViewModeAsync(CancellationToken.None);
        isLoading = false;
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
}
