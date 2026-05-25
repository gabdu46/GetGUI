using System.Collections.ObjectModel;
using System.Text.Json;
using GetGUI.Models;

namespace GetGUI.Services;

public sealed class QueueService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _queuePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GetGUI",
        "queue.json");

    public ObservableCollection<QueueItem> Items { get; } = new();

    public void Load()
    {
        try
        {
            if (File.Exists(_queuePath))
            {
                ImportFromFile(_queuePath);
            }
        }
        catch
        {
            Items.Clear();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_queuePath)!);
        ExportToFile(_queuePath);
    }

    public bool Add(ApplicationPackage application)
    {
        if (Items.Any(item => string.Equals(item.Application.Id, application.Id, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        Items.Add(new QueueItem(application.Clone()));
        Save();
        return true;
    }

    public void Remove(QueueItem item)
    {
        Items.Remove(item);
        Save();
    }

    public void Move(QueueItem item, int offset)
    {
        var oldIndex = Items.IndexOf(item);
        if (oldIndex < 0)
        {
            return;
        }

        var newIndex = Math.Clamp(oldIndex + offset, 0, Items.Count - 1);
        if (newIndex == oldIndex)
        {
            return;
        }

        Items.Move(oldIndex, newIndex);
        Save();
    }

    public void ResetFailed(QueueItem item)
    {
        item.Status = QueueItemStatus.Pending;
        item.LastMessage = string.Empty;
        Save();
    }

    public async Task InstallAllAsync(
        WingetService winget,
        bool silent,
        CancellationToken cancellationToken,
        IProgress<ProcessOutput>? progress = null)
    {
        foreach (var item in Items.Where(item => item.Status is QueueItemStatus.Pending or QueueItemStatus.Failed).ToList())
        {
            cancellationToken.ThrowIfCancellationRequested();
            item.Status = QueueItemStatus.Installing;
            item.LastMessage = "Demarrage de l'installation...";
            Save();

            progress?.Report(new ProcessOutput(false, $"==> {item.Application.Name} ({item.Application.Id})"));
            var result = await winget.InstallAsync(item.Application.Id, silent, cancellationToken, progress);

            if (result.IsSuccess)
            {
                item.Status = QueueItemStatus.Success;
                item.LastMessage = "Installation terminee.";
            }
            else
            {
                item.Status = QueueItemStatus.Failed;
                item.LastMessage = string.IsNullOrWhiteSpace(result.StandardError)
                    ? result.StandardOutput.Trim()
                    : result.StandardError.Trim();
            }

            Save();
        }
    }

    public void ExportToFile(string path)
    {
        var snapshots = Items.Select(QueueItemSnapshot.FromQueueItem).ToList();
        var json = JsonSerializer.Serialize(snapshots, JsonOptions);
        File.WriteAllText(path, json);
    }

    public void ImportFromFile(string path)
    {
        var json = File.ReadAllText(path);
        var snapshots = JsonSerializer.Deserialize<List<QueueItemSnapshot>>(json, JsonOptions) ?? new List<QueueItemSnapshot>();

        Items.Clear();
        foreach (var snapshot in snapshots)
        {
            Items.Add(snapshot.ToQueueItem());
        }
    }

    private sealed class QueueItemSnapshot
    {
        public ApplicationPackage Application { get; set; } = new();

        public QueueItemStatus Status { get; set; }

        public string LastMessage { get; set; } = string.Empty;

        public static QueueItemSnapshot FromQueueItem(QueueItem item)
        {
            return new QueueItemSnapshot
            {
                Application = item.Application.Clone(),
                Status = item.Status,
                LastMessage = item.LastMessage
            };
        }

        public QueueItem ToQueueItem()
        {
            return new QueueItem(Application.Clone())
            {
                Status = Status,
                LastMessage = LastMessage
            };
        }
    }
}
