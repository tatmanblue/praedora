using Praedora.Web.Components;
using Praedora.Web.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorBootstrap();

builder.Services.AddHttpClient<PraedoraApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://praedora-api");
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapDefaultEndpoints();

// Proxies Praedora.Api's export so the browser downloads same-origin from Praedora.Web — a
// direct browser request to the API would need CORS plus a browser-resolvable API URL, neither
// of which exists under Aspire's service discovery ("https+http://praedora-api" only resolves
// server-side).
app.MapGet("/export/applications", async (PraedoraApiClient apiClient, CancellationToken ct) =>
{
    byte[] bytes = await apiClient.ExportApplicationsAsync(ct);
    string fileName = $"praedora-export-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.json";
    return Results.File(bytes, "application/json", fileName);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
