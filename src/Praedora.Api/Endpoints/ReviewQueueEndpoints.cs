using Praedora.Application;

namespace Praedora.Api.Endpoints;

// Confirm / edit / dismiss actions on CandidateEvents (design doc §10). Deferred to build
// sequence step 4.
public static class ReviewQueueEndpoints
{
    public static void MapReviewQueueEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup(ApiRoutes.ReviewQueueBase);

        group.MapGet("/", NotImplementedResult);
        group.MapPost("/{id:guid}/confirm", NotImplementedResult);
        group.MapPost("/{id:guid}/dismiss", NotImplementedResult);
        group.MapPost("/{id:guid}/edit", NotImplementedResult);
    }

    private static IResult NotImplementedResult()
    {
        return Results.Problem("The review queue arrives in build sequence step 4.", statusCode: StatusCodes.Status501NotImplemented);
    }
}
