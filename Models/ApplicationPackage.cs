using GetGUI.Infrastructure;
using System.Text.Json.Serialization;

namespace GetGUI.Models;

public sealed class ApplicationPackage : ObservableObject
{
    private string _name = string.Empty;
    private string _id = string.Empty;
    private string _publisher = string.Empty;
    private string _description = string.Empty;
    private string _version = string.Empty;
    private string _homepage = string.Empty;
    private string _license = string.Empty;
    private string _iconUrl = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                OnPropertyChanged(nameof(Initials));
            }
        }
    }

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Publisher
    {
        get => _publisher;
        set
        {
            if (SetProperty(ref _publisher, value))
            {
                OnPropertyChanged(nameof(PublisherDisplay));
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
            {
                OnPropertyChanged(nameof(DescriptionDisplay));
            }
        }
    }

    public string Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }

    public string Homepage
    {
        get => _homepage;
        set => SetProperty(ref _homepage, value);
    }

    public string License
    {
        get => _license;
        set => SetProperty(ref _license, value);
    }

    public string IconUrl
    {
        get => _iconUrl;
        set
        {
            if (SetProperty(ref _iconUrl, value))
            {
                OnPropertyChanged(nameof(HasIcon));
            }
        }
    }

    [JsonIgnore]
    public bool HasIcon => !string.IsNullOrWhiteSpace(IconUrl);

    [JsonIgnore]
    public string Initials
    {
        get
        {
            var words = Name
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(word => char.IsLetterOrDigit(word[0]))
                .Take(2)
                .Select(word => char.ToUpperInvariant(word[0]))
                .ToArray();

            return words.Length == 0 ? "?" : new string(words);
        }
    }

    [JsonIgnore]
    public string PublisherDisplay => string.IsNullOrWhiteSpace(Publisher) ? "Editeur indisponible" : Publisher;

    [JsonIgnore]
    public string DescriptionDisplay => string.IsNullOrWhiteSpace(Description)
        ? "Description disponible dans la fiche detaillee winget."
        : Description;

    public ApplicationPackage Clone()
    {
        return new ApplicationPackage
        {
            Name = Name,
            Id = Id,
            Publisher = Publisher,
            Description = Description,
            Version = Version,
            Homepage = Homepage,
            License = License,
            IconUrl = IconUrl
        };
    }
}
