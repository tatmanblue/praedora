using Microsoft.AspNetCore.Components;
using Praedora.Application.DTOs;
using Praedora.Core.Enums;

namespace Praedora.Web.Components.Shared;

public partial class ApplicationCard : ComponentBase
{
    [Parameter, EditorRequired]
    public ApplicationSummaryDto Application { get; set; } = null!;

    [Parameter]
    public EventCallback<ApplicationStatus> StatusChanged { get; set; }

    private Task OnStatusSelected(ChangeEventArgs args)
    {
        ApplicationStatus newStatus = Enum.Parse<ApplicationStatus>((string)args.Value!);
        return StatusChanged.InvokeAsync(newStatus);
    }

    private string FormatLastActivityAge()
    {
        TimeSpan age = DateTimeOffset.UtcNow - Application.LastActivityAt;

        return age switch
        {
            { TotalMinutes: < 1 } => "just now",
            { TotalHours: < 1 } => $"{(int)age.TotalMinutes}m ago",
            { TotalDays: < 1 } => $"{(int)age.TotalHours}h ago",
            _ => $"{(int)age.TotalDays}d ago"
        };
    }
}
