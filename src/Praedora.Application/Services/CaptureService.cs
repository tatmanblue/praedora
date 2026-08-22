using Microsoft.Extensions.Logging;
using Praedora.Core.Entities;
using Praedora.Core.Interfaces;
using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Application.Services;

// Wraps ApplicationService.CaptureAsync with job-description extraction (IJobDescriptionExtractor)
// for the Chrome extension's capture path (design doc §11). Extraction is best-effort: a failure
// (e.g. no CLAUDE_API_KEY configured) is logged and does not block the capture itself.
public class CaptureService(
    ApplicationService applicationService,
    IJobDescriptionExtractor jobDescriptionExtractor,
    ILogger<CaptureService> logger)
{
    public async Task<JobApplication> CaptureFromExtensionAsync(
        string companyName, string roleTitle, string jobDescriptionRaw, string? sourceUrl, CancellationToken ct)
    {
        JobApplication application = await applicationService.CaptureAsync(
            companyName, roleTitle, jobDescriptionRaw, sourceUrl, jobDescriptionHtml: null, ct);

        try
        {
            JobDescriptionExtract jdExtract = await jobDescriptionExtractor.ExtractAsync(jobDescriptionRaw, ct);
            application = await applicationService.AttachJdExtractAsync(application.Id, jdExtract, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex, "Job description extraction failed for application {ApplicationId}; continuing without JdExtract.",
                application.Id);
        }

        return application;
    }
}
