using Praedora.Application;
using Praedora.Application.DTOs;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;

namespace Praedora.Api.Endpoints;

// App-owned runtime settings (design doc §5) — currently just the board view preference.
public static class SettingsEndpoints
{
    private const string BoardViewModeKey = "BoardViewMode";

    public static void MapSettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup(ApiRoutes.SettingsBase);

        group.MapGet("/board-view-mode", async (IAppSettingRepository appSettingRepository, CancellationToken ct) =>
        {
            string? stored = await appSettingRepository.GetAsync(BoardViewModeKey, ct);
            BoardViewMode mode = stored is not null && Enum.TryParse(stored, out BoardViewMode parsed)
                ? parsed
                : BoardViewMode.Kanban;

            return Results.Ok(new BoardViewModeDto(mode));
        });

        group.MapPut("/board-view-mode",
            async (BoardViewModeDto request, IAppSettingRepository appSettingRepository, CancellationToken ct) =>
            {
                await appSettingRepository.SetAsync(BoardViewModeKey, request.Mode.ToString(), ct);
                return Results.NoContent();
            });
    }
}
