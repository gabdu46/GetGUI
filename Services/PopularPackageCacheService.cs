using System.Text.Json;
using GetGUI.Models;

namespace GetGUI.Services;

public sealed class PopularPackageCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _cachePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GetGUI",
        "popular-cache.json");

    public IReadOnlyDictionary<string, ApplicationPackage> Load()
    {
        try
        {
            if (!File.Exists(_cachePath))
            {
                return new Dictionary<string, ApplicationPackage>();
            }

            var json = File.ReadAllText(_cachePath);
            var packages = JsonSerializer.Deserialize<List<ApplicationPackage>>(json, JsonOptions) ?? new List<ApplicationPackage>();
            return packages
                .Where(package => !string.IsNullOrWhiteSpace(package.Id))
                .GroupBy(package => package.Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, ApplicationPackage>();
        }
    }

    public void Save(IEnumerable<ApplicationPackage> packages)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_cachePath)!);
            var snapshots = packages.Select(package => package.Clone()).ToList();
            var json = JsonSerializer.Serialize(snapshots, JsonOptions);
            File.WriteAllText(_cachePath, json);
        }
        catch
        {
            // Cache failures should never block the search experience.
        }
    }
}
