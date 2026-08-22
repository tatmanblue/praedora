using Praedora.Application;

namespace Praedora.Api.Endpoints;

// Backs the Log Viewer's filterable table (design doc §9-10). Deferred to build sequence step 5.
public static class LogEndpoints
{
    public static void MapLogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(ApiRoutes.LogsBase, () =>
            Results.Problem("The Log Viewer arrives in build sequence step 5.", statusCode: StatusCodes.Status501NotImplemented));
    }
}
