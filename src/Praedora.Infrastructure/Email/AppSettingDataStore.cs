using System.Text.Json;
using Google.Apis.Util.Store;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Email;

// Backs Google's OAuth token cache with the existing AppSetting key/value table instead of a
// file on disk, consistent with design doc §5 (anything the app owns at runtime is
// database-backed config, not a separate mechanism).
public class AppSettingDataStore(IAppSettingRepository appSettingRepository) : IDataStore
{
    private const string KeyPrefix = "GmailToken:";

    public async Task StoreAsync<T>(string key, T value)
    {
        string json = JsonSerializer.Serialize(value, JsonSerializerOptions.Web);
        await appSettingRepository.SetAsync(KeyPrefix + key, json, CancellationToken.None);
    }

    public async Task DeleteAsync<T>(string key)
    {
        await appSettingRepository.DeleteAsync(KeyPrefix + key, CancellationToken.None);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        string? json = await appSettingRepository.GetAsync(KeyPrefix + key, CancellationToken.None);
        return json is null ? default : JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Web);
    }

    public Task ClearAsync()
    {
        // Not exercised by this app's flow — Praedora never offers a "forget all Google
        // credentials" action, so there is no set of keys to enumerate and clear.
        throw new NotSupportedException("Clearing all stored Gmail credentials is not supported.");
    }
}
