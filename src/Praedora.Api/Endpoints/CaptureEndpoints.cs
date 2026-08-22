using Praedora.Application;

namespace Praedora.Api.Endpoints;

// POST /api/capture — the Chrome extension's capture target (design doc §11).
// Deferred to build sequence step 3.
public static class CaptureEndpoints
{
    public static void MapCaptureEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(ApiRoutes.CaptureBase, () =>
            Results.Problem("Chrome extension capture arrives in build sequence step 3.", statusCode: StatusCodes.Status501NotImplemented));
    }
}
