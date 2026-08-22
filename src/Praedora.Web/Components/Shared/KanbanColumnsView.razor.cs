using Microsoft.AspNetCore.Components;
using Praedora.Application.DTOs;
using Praedora.Core.Enums;

namespace Praedora.Web.Components.Shared;

public partial class KanbanColumnsView : ComponentBase
{
    [Parameter]
    public List<ApplicationSummaryDto> Applications { get; set; } = [];

    [Parameter]
    public EventCallback<(Guid ApplicationId, ApplicationStatus NewStatus)> OnStatusChanged { get; set; }

    private IEnumerable<ApplicationSummaryDto> ApplicationsFor(ApplicationStatus status)
    {
        return Applications.Where(application => application.Status == status);
    }

    private int CountFor(ApplicationStatus status)
    {
        return ApplicationsFor(status).Count();
    }
}
