using Praedora.Application;
using Praedora.Application.DTOs;
using Praedora.Application.Services;

namespace Praedora.Api.Endpoints;

public static class ApplicationEndpoints
{
    public static void MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup(ApiRoutes.ApplicationsBase);

        group.MapGet("/", async (ApplicationService applicationService, CancellationToken ct) =>
        {
            List<Core.Entities.Application> board = await applicationService.GetBoardAsync(ct);
            return Results.Ok(board.Select(application => application.ToSummaryDto()));
        });

        group.MapGet("/{id:guid}", async (Guid id, ApplicationService applicationService, CancellationToken ct) =>
        {
            Core.Entities.Application? application = await applicationService.GetAsync(id, ct);
            return application is null ? Results.NotFound() : Results.Ok(application.ToDetailDto());
        });

        group.MapPost("/", async (CaptureApplicationRequest request, ApplicationService applicationService, CancellationToken ct) =>
        {
            Core.Entities.Application application = await applicationService.CaptureAsync(
                request.CompanyName, request.RoleTitle, request.JobDescriptionRaw,
                request.SourceUrl, request.JobDescriptionHtml, ct);
            return Results.Created($"{ApiRoutes.ApplicationsBase}/{application.Id}", application.ToDetailDto());
        });

        group.MapPost("/{id:guid}/status", async (Guid id, ProgressStatusRequest request, ApplicationService applicationService, CancellationToken ct) =>
        {
            Core.Entities.Application application = await applicationService.ProgressStatusAsync(
                id, request.Status, request.Note, ct);
            return Results.Ok(application.ToDetailDto());
        });

        group.MapPost("/{id:guid}/notes", async (Guid id, UpdateNotesRequest request, ApplicationService applicationService, CancellationToken ct) =>
        {
            Core.Entities.Application application = await applicationService.UpdateNotesAsync(id, request.Notes, ct);
            return Results.Ok(application.ToDetailDto());
        });

        group.MapGet("/export", async (ApplicationService applicationService, CancellationToken ct) =>
        {
            List<Core.Entities.Application> applications = await applicationService.ExportAllAsync(ct);
            return Results.Ok(applications.Select(application => application.ToDetailDto()));
        });

        group.MapDelete("/{id:guid}", async (Guid id, ApplicationService applicationService, CancellationToken ct) =>
        {
            await applicationService.DeleteAsync(id, ct);
            return Results.NoContent();
        });

        group.MapDelete("/", async (string? scope, ApplicationService applicationService, CancellationToken ct) =>
        {
            bool onlyClosed = !string.Equals(scope, "all", StringComparison.OrdinalIgnoreCase);
            int deletedCount = await applicationService.DeleteByStatusAsync(onlyClosed, ct);
            return Results.Ok(new DeleteResultDto(deletedCount));
        });
    }
}
