namespace Praedora.Application;

// Shared between Praedora.Api (which maps these) and Praedora.Web (which calls them over HTTP) —
// lives here rather than in either host project's own General/Constants.cs so the two hosts
// can't drift apart on path strings.
public static class ApiRoutes
{
    public const string ApplicationsBase = "/api/applications";
    public const string CaptureBase = "/api/capture";
    public const string ReviewQueueBase = "/api/review-queue";
    public const string LogsBase = "/api/logs";
    public const string SettingsBase = "/api/settings";
}
