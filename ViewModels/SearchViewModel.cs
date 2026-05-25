using System.Collections.ObjectModel;
using System.Text;
using GetGUI.Infrastructure;
using GetGUI.Models;
using GetGUI.Services;

namespace GetGUI.ViewModels;

public sealed class SearchViewModel : ObservableObject
{
    private readonly WingetService _winget;
    private readonly QueueService _queue;
    private readonly SettingsService _settings;
    private readonly PopularPackageCacheService _popularPackageCache;
    private CancellationTokenSource? _searchCancellation;
    private bool _popularLoaded;
    private bool _isSearching;
    private string _query = string.Empty;
    private string _statusMessage = "Saisissez un nom d'application, un editeur ou un ID winget.";
    private string _operationLog = string.Empty;

    public SearchViewModel(
        WingetService winget,
        QueueService queue,
        SettingsService settings,
        PopularPackageCacheService popularPackageCache)
    {
        _winget = winget;
        _queue = queue;
        _settings = settings;
        _popularPackageCache = popularPackageCache;
    }

    public ObservableCollection<ApplicationPackage> Results { get; } = new();

    public ObservableCollection<ApplicationPackage> PopularPackages { get; } = new();

    public string Query
    {
        get => _query;
        set => SetProperty(ref _query, value);
    }

    public bool IsSearching
    {
        get => _isSearching;
        private set => SetProperty(ref _isSearching, value);
    }

    public bool HasResults => Results.Count > 0;

    public bool HasPopularPackages => PopularPackages.Count > 0;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string OperationLog
    {
        get => _operationLog;
        private set
        {
            if (SetProperty(ref _operationLog, value))
            {
                OnPropertyChanged(nameof(HasLog));
            }
        }
    }

    public bool HasLog => !string.IsNullOrWhiteSpace(OperationLog);

    public async Task LoadPopularAsync()
    {
        if (_popularLoaded)
        {
            return;
        }

        _popularLoaded = true;
        foreach (var package in CreatePopularSeeds())
        {
            PopularPackages.Add(package);
        }

        OnPropertyChanged(nameof(HasPopularPackages));

        ApplyPopularCache();
        _ = RefreshPopularPackagesAsync(PopularPackages.ToList());
        await Task.CompletedTask;
    }

    public async Task SearchAsync(string query)
    {
        _searchCancellation?.Cancel();
        _searchCancellation = new CancellationTokenSource();
        var cancellationToken = _searchCancellation.Token;

        Query = query.Trim();
        Results.Clear();
        OnPropertyChanged(nameof(HasResults));

        if (Query.Length < 2)
        {
            StatusMessage = "Entrez au moins 2 caracteres pour lancer la recherche winget.";
            return;
        }

        try
        {
            IsSearching = true;
            StatusMessage = "Recherche winget en cours...";
            var packages = await _winget.SearchAsync(Query, cancellationToken);

            foreach (var package in packages)
            {
                Results.Add(package);
            }

            OnPropertyChanged(nameof(HasResults));
            StatusMessage = packages.Count == 0
                ? "Aucun resultat winget pour cette recherche."
                : $"{packages.Count} resultat(s) depuis la source winget.";

            _ = EnrichVisibleResultsAsync(packages.Take(10).ToList(), cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsSearching = false;
        }
    }

    public bool AddToQueue(ApplicationPackage package)
    {
        var added = _queue.Add(package);
        StatusMessage = added
            ? $"{package.Name} ajoute a la file."
            : $"{package.Name} est deja dans la file.";
        return added;
    }

    public async Task InstallAsync(ApplicationPackage package, bool silent)
    {
        var builder = new StringBuilder();
        var progress = new Progress<ProcessOutput>(line =>
        {
            if (!_settings.Current.KeepInstallLogs)
            {
                return;
            }

            builder.AppendLine(line.IsError ? "[err] " + line.Text : line.Text);
            OperationLog = builder.ToString();
        });

        try
        {
            StatusMessage = $"Installation de {package.Name}...";
            var result = await _winget.InstallAsync(package.Id, silent, CancellationToken.None, progress);
            StatusMessage = result.IsSuccess
                ? $"{package.Name} installe."
                : $"Echec de l'installation de {package.Name}.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task EnrichVisibleResultsAsync(IReadOnlyList<ApplicationPackage> packages, CancellationToken cancellationToken)
    {
        foreach (var package in packages)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var details = await _winget.ShowAsync(package.Id, cancellationToken);
                package.Publisher = details.Publisher;
                package.Description = details.Description;
                package.Homepage = details.Homepage;
                package.License = details.License;
                package.IconUrl = details.IconUrl;
                if (!string.IsNullOrWhiteSpace(details.Version))
                {
                    package.Version = details.Version;
                }
            }
            catch
            {
                // Enrichment is opportunistic: search results stay usable without details.
            }
        }
    }

    private void ApplyPopularCache()
    {
        var cache = _popularPackageCache.Load();
        foreach (var package in PopularPackages)
        {
            if (cache.TryGetValue(package.Id, out var cachedPackage))
            {
                ApplyDetails(package, cachedPackage, preserveDisplayName: true);
            }
        }
    }

    private async Task RefreshPopularPackagesAsync(IReadOnlyList<ApplicationPackage> packages)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Query))
            {
                StatusMessage = "Mise a jour rapide des details populaires...";
            }

            await EnrichPopularPackagesAsync(packages);
            _popularPackageCache.Save(packages);

            if (string.IsNullOrWhiteSpace(Query))
            {
                StatusMessage = "Saisissez un nom d'application, un editeur ou un ID winget.";
            }
        }
        catch
        {
            if (string.IsNullOrWhiteSpace(Query))
            {
                StatusMessage = "Saisissez un nom d'application, un editeur ou un ID winget.";
            }
        }
    }

    private async Task EnrichPopularPackagesAsync(IReadOnlyList<ApplicationPackage> packages)
    {
        using var limiter = new SemaphoreSlim(12);
        var tasks = packages.Select(package => EnrichPopularPackageAsync(package, limiter)).ToList();
        await Task.WhenAll(tasks);
    }

    private async Task EnrichPopularPackageAsync(ApplicationPackage package, SemaphoreSlim limiter)
    {
        await limiter.WaitAsync();
        try
        {
            var details = await _winget.ShowAsync(package.Id, CancellationToken.None);
            ApplyDetails(package, details, preserveDisplayName: true);
        }
        catch
        {
            // Popular tiles are a convenience layer; the search experience remains primary.
        }
        finally
        {
            limiter.Release();
        }
    }

    private static void ApplyDetails(ApplicationPackage target, ApplicationPackage details, bool preserveDisplayName)
    {
        if (!preserveDisplayName && !string.IsNullOrWhiteSpace(details.Name) && !LooksLikePackageId(details.Name, details.Id))
        {
            target.Name = details.Name;
        }

        if (!string.IsNullOrWhiteSpace(details.Publisher))
        {
            target.Publisher = details.Publisher;
        }

        if (!string.IsNullOrWhiteSpace(details.Description))
        {
            target.Description = details.Description;
        }

        if (!string.IsNullOrWhiteSpace(details.Version))
        {
            target.Version = details.Version;
        }

        if (!string.IsNullOrWhiteSpace(details.Homepage))
        {
            target.Homepage = details.Homepage;
        }

        if (!string.IsNullOrWhiteSpace(details.License))
        {
            target.License = details.License;
        }

        if (!string.IsNullOrWhiteSpace(details.IconUrl))
        {
            target.IconUrl = details.IconUrl;
        }
    }

    private static bool LooksLikePackageId(string name, string id)
    {
        return string.Equals(name, id, StringComparison.OrdinalIgnoreCase)
            || (name.Contains('.', StringComparison.Ordinal) && !name.Contains(' ', StringComparison.Ordinal));
    }

    private static IReadOnlyList<ApplicationPackage> CreatePopularSeeds()
    {
        return new[]
        {
            CreateSeed("Mozilla Firefox", "Mozilla.Firefox", "Navigateur web"),
            CreateSeed("Google Chrome", "Google.Chrome", "Navigateur web"),
            CreateSeed("Microsoft Edge", "Microsoft.Edge", "Navigateur web"),
            CreateSeed("Brave Browser", "Brave.Brave", "Navigateur web"),
            CreateSeed("Opera", "Opera.Opera", "Navigateur web"),
            CreateSeed("Microsoft PowerToys", "Microsoft.PowerToys", "Utilitaires Windows"),
            CreateSeed("7-Zip", "7zip.7zip", "Compression de fichiers"),
            CreateSeed("WinRAR", "RARLab.WinRAR", "Compression de fichiers"),
            CreateSeed("VLC media player", "VideoLAN.VLC", "Lecteur multimedia"),
            CreateSeed("Visual Studio Code", "Microsoft.VisualStudioCode", "Editeur de code"),
            CreateSeed("Visual Studio Community", "Microsoft.VisualStudio.2022.Community", "IDE Microsoft"),
            CreateSeed("Git", "Git.Git", "Controle de version"),
            CreateSeed("GitHub Desktop", "GitHub.GitHubDesktop", "Client Git"),
            CreateSeed("Docker Desktop", "Docker.DockerDesktop", "Conteneurs"),
            CreateSeed("Windows Terminal", "Microsoft.WindowsTerminal", "Terminal"),
            CreateSeed("PowerShell", "Microsoft.PowerShell", "Shell moderne"),
            CreateSeed("Oh My Posh", "JanDeDobbeleer.OhMyPosh", "Prompt terminal"),
            CreateSeed("Node.js LTS", "OpenJS.NodeJS.LTS", "Runtime JavaScript"),
            CreateSeed("Python", "Python.Python.3.13", "Langage Python"),
            CreateSeed("Go", "GoLang.Go", "Langage Go"),
            CreateSeed("Rustup", "Rustlang.Rustup", "Toolchain Rust"),
            CreateSeed("Postman", "Postman.Postman", "API client"),
            CreateSeed("Insomnia", "Insomnia.Insomnia", "API client"),
            CreateSeed("DBeaver Community", "DBeaver.DBeaver.Community", "Base de donnees"),
            CreateSeed("MongoDB Compass", "MongoDB.Compass.Full", "Base de donnees"),
            CreateSeed("Discord", "Discord.Discord", "Communication"),
            CreateSeed("Slack", "SlackTechnologies.Slack", "Communication"),
            CreateSeed("Microsoft Teams", "Microsoft.Teams", "Communication"),
            CreateSeed("Zoom", "Zoom.Zoom", "Visioconference"),
            CreateSeed("Beeper", "Beeper.Beeper", "Messagerie"),
            CreateSeed("Telegram Desktop", "Telegram.TelegramDesktop", "Messagerie"),
            CreateSeed("Steam", "Valve.Steam", "Jeux"),
            CreateSeed("Epic Games Launcher", "EpicGames.EpicGamesLauncher", "Jeux"),
            CreateSeed("GOG Galaxy", "GOG.Galaxy", "Jeux"),
            CreateSeed("EA app", "ElectronicArts.EADesktop", "Jeux"),
            CreateSeed("Ubisoft Connect", "Ubisoft.Connect", "Jeux"),
            CreateSeed("Spotify", "Spotify.Spotify", "Musique"),
            CreateSeed("iTunes", "Apple.iTunes", "Musique"),
            CreateSeed("OBS Studio", "OBSProject.OBSStudio", "Capture et streaming"),
            CreateSeed("Audacity", "Audacity.Audacity", "Audio"),
            CreateSeed("Krita", "KDE.Krita", "Dessin"),
            CreateSeed("GIMP", "GIMP.GIMP", "Image"),
            CreateSeed("Inkscape", "Inkscape.Inkscape", "Vectoriel"),
            CreateSeed("Blender", "BlenderFoundation.Blender", "3D"),
            CreateSeed("ShareX", "ShareX.ShareX", "Capture ecran"),
            CreateSeed("Lightshot", "Skillbrains.Lightshot", "Capture ecran"),
            CreateSeed("Everything", "voidtools.Everything", "Recherche fichiers"),
            CreateSeed("Files", "FilesCommunity.Files", "Explorateur moderne"),
            CreateSeed("EarTrumpet", "File-New-Project.EarTrumpet", "Volume audio"),
            CreateSeed("AutoHotkey", "AutoHotkey.AutoHotkey", "Automatisation"),
            CreateSeed("KeePassXC", "KeePassXCTeam.KeePassXC", "Mots de passe"),
            CreateSeed("Bitwarden", "Bitwarden.Bitwarden", "Mots de passe"),
            CreateSeed("1Password", "AgileBits.1Password", "Mots de passe"),
            CreateSeed("qBittorrent", "qBittorrent.qBittorrent", "Torrent"),
            CreateSeed("Cyberduck", "Iterate.Cyberduck", "FTP et SFTP"),
            CreateSeed("WinSCP", "WinSCP.WinSCP", "SFTP"),
            CreateSeed("PuTTY", "PuTTY.PuTTY", "SSH"),
            CreateSeed("Rufus", "Rufus.Rufus", "USB bootable"),
            CreateSeed("balenaEtcher", "Balena.Etcher", "USB bootable"),
            CreateSeed("CrystalDiskInfo", "CrystalDewWorld.CrystalDiskInfo", "Disques"),
            CreateSeed("CrystalDiskMark", "CrystalDewWorld.CrystalDiskMark", "Benchmark disque"),
            CreateSeed("HWMonitor", "CPUID.HWMonitor", "Monitoring PC"),
            CreateSeed("CPU-Z", "CPUID.CPU-Z", "Infos systeme"),
            CreateSeed("GPU-Z", "TechPowerUp.GPU-Z", "Infos GPU"),
            CreateSeed("HWiNFO", "REALiX.HWiNFO", "Infos systeme"),
            CreateSeed("Notion", "Notion.Notion", "Notes"),
            CreateSeed("Obsidian", "Obsidian.Obsidian", "Notes"),
            CreateSeed("Joplin", "Joplin.Joplin", "Notes"),
            CreateSeed("LibreOffice", "TheDocumentFoundation.LibreOffice", "Suite bureautique"),
            CreateSeed("Adobe Acrobat Reader", "Adobe.Acrobat.Reader.64-bit", "PDF"),
            CreateSeed("SumatraPDF", "SumatraPDF.SumatraPDF", "PDF"),
            CreateSeed("Notepad++", "Notepad++.Notepad++", "Editeur texte"),
            CreateSeed("Sublime Text", "SublimeHQ.SublimeText.4", "Editeur texte"),
            CreateSeed("JetBrains Toolbox", "JetBrains.Toolbox", "IDE JetBrains"),
            CreateSeed("Figma", "Figma.Figma", "Design"),
            CreateSeed("Canva", "Canva.Canva", "Design"),
            CreateSeed("NVIDIA GeForce NOW", "Nvidia.GeForceNow", "Cloud gaming"),
            CreateSeed("Logitech G HUB", "Logitech.GHUB", "Peripheriques"),
            CreateSeed("Signal", "OpenWhisperSystems.Signal", "Messagerie")
        };
    }

    private static ApplicationPackage CreateSeed(string name, string id, string description)
    {
        return new ApplicationPackage
        {
            Name = name,
            Id = id,
            Description = description
        };
    }
}
