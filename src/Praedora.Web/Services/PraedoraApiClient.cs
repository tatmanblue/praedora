using System.Net;
using System.Net.Http.Json;
using Praedora.Application;
using Praedora.Application.DTOs;
using Praedora.Core.Enums;

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

    public async Task DeleteApplicationAsync(Guid id, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{ApiRoutes.ApplicationsBase}/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<int> DeleteApplicationsAsync(string scope, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync(
            $"{ApiRoutes.ApplicationsBase}?scope={Uri.EscapeDataString(scope)}", ct);
        response.EnsureSuccessStatusCode();
        DeleteResultDto? result = await response.Content.ReadFromJsonAsync<DeleteResultDto>(ct);
        return result?.DeletedCount ?? 0;
    }

    public async Task<byte[]> ExportApplicationsAsync(CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"{ApiRoutes.ApplicationsBase}/export", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    public async Task<List<CandidateEventDto>> GetPendingReviewQueueAsync(CancellationToken ct)
    {
        List<CandidateEventDto>? pending = await httpClient.GetFromJsonAsync<List<CandidateEventDto>>(
            ApiRoutes.ReviewQueueBase, ct);
        return pending ?? [];
    }

    public async Task<SyncStatusDto?> GetSyncStatusAsync(CancellationToken ct)
    {
        return await httpClient.GetFromJsonAsync<SyncStatusDto>($"{ApiRoutes.ReviewQueueBase}/sync-status", ct);
    }

    public async Task ConfirmCandidateEventAsync(Guid id, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.PostAsync($"{ApiRoutes.ReviewQueueBase}/{id}/confirm", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DismissCandidateEventAsync(Guid id, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.PostAsync($"{ApiRoutes.ReviewQueueBase}/{id}/dismiss", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task EditCandidateEventAsync(Guid id, EditCandidateEventRequest request, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{ApiRoutes.ReviewQueueBase}/{id}/edit", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task TriggerSyncAsync(CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.PostAsync($"{ApiRoutes.ReviewQueueBase}/sync", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<LogEntryDto>> GetLogsAsync(
        string? severity, string? component, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct)
    {
        Dictionary<string, string?> query = new()
        {
            ["severity"] = severity,
            ["component"] = component,
            ["from"] = from?.ToString("O"),
            ["to"] = to?.ToString("O")
        };
        string queryString = string.Join('&', query
            .Where(pair => !string.IsNullOrEmpty(pair.Value))
            .Select(pair => $"{pair.Key}={Uri.EscapeDataString(pair.Value!)}"));

        List<LogEntryDto>? entries = await httpClient.GetFromJsonAsync<List<LogEntryDto>>(
            $"{ApiRoutes.LogsBase}?{queryString}", ct);
        return entries ?? [];
    }

    public async Task ClearLogsAsync(CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync(ApiRoutes.LogsBase, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<BoardViewMode> GetBoardViewModeAsync(CancellationToken ct)
    {
        BoardViewModeDto? dto = await httpClient.GetFromJsonAsync<BoardViewModeDto>(
            $"{ApiRoutes.SettingsBase}/board-view-mode", ct);
        return dto?.Mode ?? BoardViewMode.Kanban;
    }

    public async Task SetBoardViewModeAsync(BoardViewMode mode, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync(
            $"{ApiRoutes.SettingsBase}/board-view-mode", new BoardViewModeDto(mode), ct);
        response.EnsureSuccessStatusCode();
    }
}
