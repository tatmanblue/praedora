using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Application.Services;

// Will wrap ApplicationService.CaptureAsync with job-description extraction (IJobDescriptionExtractor)
// for the Chrome extension's capture path (design doc §11). Deferred to build sequence step 3.
public class CaptureService
{
    public Task<JobApplication> CaptureFromExtensionAsync(
        string companyName, string roleTitle, string jobDescriptionRaw, string sourceUrl, CancellationToken ct)
    {
        throw new NotImplementedException("Chrome extension capture arrives in build sequence step 3.");
    }
}
