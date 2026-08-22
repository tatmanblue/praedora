namespace Praedora.Core.Interfaces;

// Runtime/database-backed configuration (design doc §5) — distinct from the .env bootstrap
// values. Backs both app-owned settings (e.g. sync interval) and the Gmail OAuth token cache.
public interface IAppSettingRepository
{
    Task<string?> GetAsync(string key, CancellationToken ct);
    Task SetAsync(string key, string value, CancellationToken ct);
    Task DeleteAsync(string key, CancellationToken ct);
}
