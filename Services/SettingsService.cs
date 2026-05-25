using System.Text.Json;

namespace GetGUI.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GetGUI",
        "settings.json");

    public AppSettings Current { get; private set; } = new();

    public void Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return;
            }

            var json = File.ReadAllText(_settingsPath);
            Current = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch
        {
            Current = new AppSettings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        var json = JsonSerializer.Serialize(Current, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }
}
