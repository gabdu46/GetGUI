using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GetGUI.Models;

namespace GetGUI.Services;

public sealed class WingetService
{
    private readonly ProcessRunner _processRunner;

    public WingetService(ProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<IReadOnlyList<ApplicationPackage>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<ApplicationPackage>();
        }

        var result = await _processRunner.RunAsync(
            "winget",
            new[]
            {
                "search",
                "--source",
                "winget",
                "--accept-source-agreements",
                "--disable-interactivity",
                query
            },
            cancellationToken,
            logOutput: false);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(CleanCliOutput(result.CombinedOutput));
        }

        return ParseSearchResults(result.StandardOutput);
    }

    public async Task<ApplicationPackage> ShowAsync(string id, CancellationToken cancellationToken, bool logOutput = false)
    {
        var result = await _processRunner.RunAsync(
            "winget",
            new[]
            {
                "show",
                "--id",
                id,
                "--exact",
                "--source",
                "winget",
                "--accept-source-agreements",
                "--disable-interactivity"
            },
            cancellationToken,
            logOutput: logOutput);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(CleanCliOutput(result.CombinedOutput));
        }

        return ParseShowResult(result.StandardOutput, id);
    }

    public Task<ProcessResult> InstallAsync(
        string id,
        bool silent,
        CancellationToken cancellationToken,
        IProgress<ProcessOutput>? progress = null)
    {
        var arguments = new List<string>
        {
            "install",
            "--id",
            id,
            "--exact",
            "--source",
            "winget",
            "--accept-source-agreements",
            "--accept-package-agreements",
            "--disable-interactivity"
        };

        if (silent)
        {
            arguments.Add("--silent");
        }

        return _processRunner.RunAsync("winget", arguments, cancellationToken, progress);
    }

    public Task<ProcessResult> UninstallAsync(
        string id,
        CancellationToken cancellationToken,
        IProgress<ProcessOutput>? progress = null)
    {
        return _processRunner.RunAsync(
            "winget",
            new[]
            {
                "uninstall",
                "--id",
                id,
                "--exact",
                "--disable-interactivity"
            },
            cancellationToken,
            progress);
    }

    private static IReadOnlyList<ApplicationPackage> ParseSearchResults(string text)
    {
        var lines = text
            .Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.TrimEnd())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var headerIndex = lines.FindIndex(IsSearchHeader);
        if (headerIndex < 0)
        {
            return ParseSearchResultsWithRegex(lines);
        }

        var header = lines[headerIndex];
        var idIndex = IndexOfColumn(header, "id");
        var versionIndex = IndexOfColumn(header, "version");
        var sourceIndex = FirstPositive(
            IndexOfColumn(header, "source"),
            IndexOfColumn(header, "correspondance"),
            IndexOfColumn(header, "match"));

        if (idIndex <= 0 || versionIndex <= idIndex)
        {
            return ParseSearchResultsWithRegex(lines.Skip(headerIndex + 1));
        }

        var packages = new List<ApplicationPackage>();
        foreach (var line in lines.Skip(headerIndex + 1))
        {
            if (line.Trim().All(character => character == '-' || character == ' '))
            {
                continue;
            }

            if (line.Length <= idIndex)
            {
                continue;
            }

            var name = SliceColumn(line, 0, idIndex);
            var id = SliceColumn(line, idIndex, versionIndex);
            var versionEnd = sourceIndex > versionIndex ? sourceIndex : line.Length;
            var version = SliceColumn(line, versionIndex, versionEnd);

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            packages.Add(new ApplicationPackage
            {
                Name = name,
                Id = id,
                Version = version
            });
        }

        return packages
            .GroupBy(package => package.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(120)
            .ToList();
    }

    private static IReadOnlyList<ApplicationPackage> ParseSearchResultsWithRegex(IEnumerable<string> lines)
    {
        var packages = new List<ApplicationPackage>();
        var regex = new Regex(@"^(?<name>.+?)\s{2,}(?<id>[A-Za-z0-9][A-Za-z0-9._-]+)\s{2,}(?<version>\S+)", RegexOptions.Compiled);

        foreach (var line in lines)
        {
            var match = regex.Match(line);
            if (!match.Success)
            {
                continue;
            }

            packages.Add(new ApplicationPackage
            {
                Name = match.Groups["name"].Value.Trim(),
                Id = match.Groups["id"].Value.Trim(),
                Version = match.Groups["version"].Value.Trim()
            });
        }

        return packages;
    }

    private static ApplicationPackage ParseShowResult(string text, string fallbackId)
    {
        var package = new ApplicationPackage { Id = fallbackId };
        var currentMappedKey = string.Empty;

        foreach (var rawLine in text.Replace("\r", string.Empty).Split('\n'))
        {
            var line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var keyValueMatch = Regex.Match(line, @"^\s{0,8}(?<key>[^:]+):\s*(?<value>.*)$");
            if (keyValueMatch.Success)
            {
                var key = NormalizeKey(keyValueMatch.Groups["key"].Value);
                var value = keyValueMatch.Groups["value"].Value.Trim();
                currentMappedKey = ApplyShowValue(package, key, value);
                continue;
            }

            if (string.IsNullOrWhiteSpace(package.Name))
            {
                ApplyFoundLine(package, line, fallbackId);
                continue;
            }

            if (currentMappedKey == "description")
            {
                package.Description = Append(package.Description, line.Trim());
            }

            if (!package.HasIcon)
            {
                var iconUrl = FindIconUrl(line);
                if (!string.IsNullOrWhiteSpace(iconUrl))
                {
                    package.IconUrl = iconUrl;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(package.Name))
        {
            package.Name = fallbackId;
        }

        return package;
    }

    private static void ApplyFoundLine(ApplicationPackage package, string line, string fallbackId)
    {
        var match = Regex.Match(line, @"(?<name>.+?)\s+\[(?<id>[^\]]+)\]");
        if (match.Success)
        {
            var name = match.Groups["name"].Value.Trim(':', ' ');
            var normalized = NormalizeKey(name);
            if (normalized.StartsWith("found", StringComparison.Ordinal) || normalized.StartsWith("trouve", StringComparison.Ordinal))
            {
                var firstSpace = name.IndexOf(' ');
                if (firstSpace >= 0 && firstSpace < name.Length - 1)
                {
                    name = name[(firstSpace + 1)..];
                }
            }

            package.Name = name.Trim(':', ' ');
            package.Id = match.Groups["id"].Value.Trim();
        }
        else
        {
            package.Name = fallbackId;
        }
    }

    private static string ApplyShowValue(ApplicationPackage package, string normalizedKey, string value)
    {
        switch (normalizedKey)
        {
            case "name":
            case "nom":
                package.Name = value;
                return "name";
            case "version":
                package.Version = value;
                return "version";
            case "publisher":
            case "editeur":
            case "author":
            case "auteur":
                if (string.IsNullOrWhiteSpace(package.Publisher))
                {
                    package.Publisher = value;
                }

                return "publisher";
            case "description":
                package.Description = value;
                return "description";
            case "homepage":
            case "siteweb":
            case "pagedaccueil":
            case "pageaccueil":
                package.Homepage = value;
                return "homepage";
            case "license":
            case "licence":
                package.License = value;
                return "license";
            case "icon":
            case "icons":
            case "icone":
            case "icones":
                package.IconUrl = FindIconUrl(value);
                return "icon";
            default:
                if (!package.HasIcon)
                {
                    package.IconUrl = FindIconUrl(value);
                }

                return normalizedKey;
        }
    }

    private static bool IsSearchHeader(string line)
    {
        var normalized = NormalizeKey(line);
        return normalized.Contains("id", StringComparison.Ordinal)
            && normalized.Contains("version", StringComparison.Ordinal)
            && (normalized.Contains("name", StringComparison.Ordinal) || normalized.Contains("nom", StringComparison.Ordinal));
    }

    private static int IndexOfColumn(string header, string columnName)
    {
        var match = Regex.Match(header, $@"\b{Regex.Escape(columnName)}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? match.Index : -1;
    }

    private static int FirstPositive(params int[] values)
    {
        return values.FirstOrDefault(value => value >= 0, -1);
    }

    private static string SliceColumn(string line, int start, int end)
    {
        if (start >= line.Length)
        {
            return string.Empty;
        }

        var safeEnd = Math.Min(end, line.Length);
        return line[start..safeEnd].Trim();
    }

    private static string NormalizeKey(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString();
    }

    private static string FindIconUrl(string text)
    {
        var match = Regex.Match(text, @"https?://\S+\.(?:png|jpg|jpeg|ico|webp)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? match.Value.TrimEnd('.', ',', ';') : string.Empty;
    }

    private static string Append(string current, string next)
    {
        if (string.IsNullOrWhiteSpace(next))
        {
            return current;
        }

        return string.IsNullOrWhiteSpace(current) ? next : current + Environment.NewLine + next;
    }

    private static string CleanCliOutput(string output)
    {
        return string.IsNullOrWhiteSpace(output)
            ? "La commande winget n'a retourne aucun detail."
            : output.Trim();
    }
}
