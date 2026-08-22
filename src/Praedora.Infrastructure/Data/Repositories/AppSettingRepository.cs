using Microsoft.EntityFrameworkCore;
using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Data.Repositories;

public class AppSettingRepository(PraedoraDbContext dbContext) : IAppSettingRepository
{
    public async Task<string?> GetAsync(string key, CancellationToken ct)
    {
        AppSetting? setting = await dbContext.AppSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
        return setting?.Value;
    }

    public async Task SetAsync(string key, string value, CancellationToken ct)
    {
        AppSetting? setting = await dbContext.AppSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is null)
        {
            await dbContext.AppSettings.AddAsync(AppSetting.Create(key, value, DateTimeOffset.UtcNow), ct);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(string key, CancellationToken ct)
    {
        AppSetting? setting = await dbContext.AppSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is not null)
        {
            dbContext.AppSettings.Remove(setting);
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
