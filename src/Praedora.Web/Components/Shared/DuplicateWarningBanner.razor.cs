using Microsoft.AspNetCore.Components;

namespace Praedora.Web.Components.Shared;

public partial class DuplicateWarningBanner : ComponentBase
{
    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public string CompanyName { get; set; } = "";
}
