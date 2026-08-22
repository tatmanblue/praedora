using Praedora.Application;
using Praedora.Application.DTOs;
using Praedora.Application.Services;

namespace Praedora.Api.Endpoints;

// POST /api/capture — the Chrome extension's capture target (design doc §11).
public static class CaptureEndpoints
{
    public const string CorsPolicyName = "Extension";

    public static void MapCaptureEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(ApiRoutes.CaptureBase,
                async (CaptureApplicationRequest request, CaptureService captureService, CancellationToken ct) =>
                {
                    Core.Entities.Application application = await captureService.CaptureFromExtensionAsync(
                        request.CompanyName, request.RoleTitle, request.JobDescriptionRaw, request.SourceUrl, ct);
                    return Results.Created($"{ApiRoutes.ApplicationsBase}/{application.Id}", application.ToDetailDto());
                })
            .RequireCors(CorsPolicyName);
    }
}
