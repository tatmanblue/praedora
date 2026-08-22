namespace Praedora.Web.General;

// Named PageRoutes, not Routes, to avoid colliding with the generated Routes.razor router component.
public static class PageRoutes
{
    public const string Board = "/";
    public const string ApplicationDetail = "/applications/{Id:guid}";

    public static string ApplicationDetailPath(Guid id)
    {
        return $"/applications/{id}";
    }
}
