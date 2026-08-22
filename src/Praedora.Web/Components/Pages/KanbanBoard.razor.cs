using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Praedora.Application.DTOs;
using Praedora.Core.Enums;
using Praedora.Web.Services;

namespace Praedora.Web.Components.Pages;

public partial class KanbanBoard : ComponentBase
{
    [Inject]
    public PraedoraApiClient ApiClient { get; set; } = null!;

    private List<ApplicationSummaryDto> applications = [];
    private bool isLoading = true;
    private bool isSubmitting;
    private bool showCaptureForm;
    private CaptureFormModel captureForm = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadBoardAsync();
    }

    private async Task LoadBoardAsync()
    {
        isLoading = true;
        applications = await ApiClient.GetBoardAsync(CancellationToken.None);
        isLoading = false;
    }

    private IEnumerable<ApplicationSummaryDto> ApplicationsFor(ApplicationStatus status)
    {
        return applications.Where(application => application.Status == status);
    }

    private int ColumnCount(ApplicationStatus status)
    {
        return ApplicationsFor(status).Count();
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

    private async Task OnCardStatusChangedAsync(Guid applicationId, ApplicationStatus newStatus)
    {
        await ApiClient.ProgressStatusAsync(applicationId, new ProgressStatusRequest(newStatus, null), CancellationToken.None);
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
