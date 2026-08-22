using Praedora.Application;
using Praedora.Application.DTOs;
using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Api.Endpoints;

// Backs the Log Viewer's filterable table (design doc §9-10).
public static class LogEndpoints
{
    public static void MapLogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(ApiRoutes.LogsBase,
            async (string? severity, string? component, DateTimeOffset? from, DateTimeOffset? to,
                ILogEntryRepository logEntryRepository, CancellationToken ct) =>
            {
                List<LogEntry> entries = await logEntryRepository.QueryAsync(severity, component, from, to, ct);
                return Results.Ok(entries.Select(entry => entry.ToDto()));
            });

        endpoints.MapDelete(ApiRoutes.LogsBase, async (ILogEntryRepository logEntryRepository, CancellationToken ct) =>
        {
            await logEntryRepository.ClearAllAsync(ct);
            return Results.NoContent();
        });
    }
}
