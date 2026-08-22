using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Praedora.Application.DTOs;
using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;
using Xunit;

namespace Praedora.IntegrationTests;

public class CaptureEndpointTests : IClassFixture<CaptureEndpointTests.PraedoraApiFactory>, IDisposable
{
    private readonly PraedoraApiFactory factory;

    public CaptureEndpointTests(PraedoraApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task PostCapture_ReturnsCreatedApplication_WithCapturedStatus()
    {
        HttpClient client = factory.CreateClient();
        CaptureApplicationRequest request = new(
            "Acme Corp", "Backend Engineer", "Do backend things.", "https://example.com/jobs/1", null);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/capture", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        ApplicationDetailDto? created = await response.Content.ReadFromJsonAsync<ApplicationDetailDto>();
        Assert.NotNull(created);
        Assert.Equal("Acme Corp", created.CompanyName);
        Assert.Equal(ApplicationStatus.Captured, created.Status);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public class PraedoraApiFactory : WebApplicationFactory<Program>
    {
        private readonly string dbPath = Path.Combine(Path.GetTempPath(), $"praedora-tests-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PRAEDORA_DB_PROVIDER"] = "Sqlite",
                ["PRAEDORA_DB_CONNECTION"] = $"Data Source={dbPath}"
            }));

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IJobDescriptionExtractor>();
                services.AddScoped<IJobDescriptionExtractor, StubJobDescriptionExtractor>();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    private class StubJobDescriptionExtractor : IJobDescriptionExtractor
    {
        public Task<JobDescriptionExtract> ExtractAsync(string rawJobDescription, CancellationToken ct)
        {
            return Task.FromResult(new JobDescriptionExtract([], [], null, null, null, null));
        }
    }
}
