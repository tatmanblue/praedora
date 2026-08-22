namespace Praedora.Core.Entities;

// Runtime/database-backed configuration the app can change without a restart, distinct from
// the .env bootstrap values needed to reach the database and external providers in the first
// place (design doc §5).
public class AppSetting
{
    public string Key { get; private set; } = "";
    public string Value { get; set; } = "";
    public DateTimeOffset UpdatedAt { get; set; }

    private AppSetting()
    {
    }

    public static AppSetting Create(string key, string value, DateTimeOffset updatedAt)
    {
        return new AppSetting
        {
            Key = key,
            Value = value,
            UpdatedAt = updatedAt
        };
    }
}
