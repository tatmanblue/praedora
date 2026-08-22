using Microsoft.AspNetCore.Components;
using Praedora.Application.DTOs;
using Praedora.Core.Enums;

namespace Praedora.Web.Components.Shared;

public partial class StackedStatusView : ComponentBase
{
    [Parameter]
    public List<ApplicationSummaryDto> Applications { get; set; } = [];

    [Parameter]
    public EventCallback<(Guid ApplicationId, ApplicationStatus NewStatus)> OnStatusChanged { get; set; }

    // Defaulted once per status (expanded unless it started empty), then left alone — a poll
    // refresh that changes a status's count shouldn't silently flip a card the user already
    // expanded or collapsed by hand.
    private readonly Dictionary<ApplicationStatus, bool> expanded = [];

    protected override void OnParametersSet()
    {
        foreach (ApplicationStatus status in Enum.GetValues<ApplicationStatus>())
        {
            expanded.TryAdd(status, CountFor(status) > 0);
        }
    }

    private bool IsExpanded(ApplicationStatus status)
    {
        return expanded.TryGetValue(status, out bool value) && value;
    }

    private void Toggle(ApplicationStatus status)
    {
        expanded[status] = !IsExpanded(status);
    }

    private IEnumerable<ApplicationSummaryDto> ApplicationsFor(ApplicationStatus status)
    {
        return Applications.Where(application => application.Status == status);
    }

    private int CountFor(ApplicationStatus status)
    {
        return ApplicationsFor(status).Count();
    }
}
