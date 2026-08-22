using System.Net;
using System.Net.Http.Json;
using Praedora.Application;
using Praedora.Application.DTOs;

namespace Praedora.Web.Services;

public class PraedoraApiClient(HttpClient httpClient)
{
    public async Task<List<ApplicationSummaryDto>> GetBoardAsync(CancellationToken ct)
    {
        List<ApplicationSummaryDto>? board = await httpClient.GetFromJsonAsync<List<ApplicationSummaryDto>>(
            ApiRoutes.ApplicationsBase, ct);
        return board ?? [];
    }

    public async Task<ApplicationDetailDto?> GetApplicationAsync(Guid id, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"{ApiRoutes.ApplicationsBase}/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplicationDetailDto>(ct);
    }

    public async Task<ApplicationDetailDto> CaptureAsync(CaptureApplicationRequest request, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(ApiRoutes.ApplicationsBase, request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApplicationDetailDto>(ct))!;
    }

    public async Task<ApplicationDetailDto> ProgressStatusAsync(Guid id, ProgressStatusRequest request, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"{ApiRoutes.ApplicationsBase}/{id}/status", request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApplicationDetailDto>(ct))!;
    }

    public async Task<ApplicationDetailDto> UpdateNotesAsync(Guid id, UpdateNotesRequest request, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"{ApiRoutes.ApplicationsBase}/{id}/notes", request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApplicationDetailDto>(ct))!;
    }
}
