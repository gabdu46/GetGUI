using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using GetGUI.Infrastructure;
using GetGUI.Models;
using GetGUI.Services;

namespace GetGUI.ViewModels;

public sealed class QueueViewModel : ObservableObject
{
    private readonly QueueService _queue;
    private readonly WingetService _winget;
    private readonly SettingsService _settings;
    private bool _isInstalling;
    private string _statusMessage = LocalizationService.Current.QueueLocalMessage;
    private string _installLog = string.Empty;

    public QueueViewModel(QueueService queue, WingetService winget, SettingsService settings)
    {
        _queue = queue;
        _winget = winget;
        _settings = settings;
        Items.CollectionChanged += Items_CollectionChanged;
    }

    public ObservableCollection<QueueItem> Items => _queue.Items;

    public bool IsInstalling
    {
        get => _isInstalling;
        private set => SetProperty(ref _isInstalling, value);
    }

    public bool HasItems => Items.Count > 0;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string InstallLog
    {
        get => _installLog;
        private set
        {
            if (SetProperty(ref _installLog, value))
            {
                OnPropertyChanged(nameof(HasLog));
            }
        }
    }

    public bool HasLog => !string.IsNullOrWhiteSpace(InstallLog);

    public async Task InstallAllAsync(bool silent)
    {
        if (Items.Count == 0)
        {
            StatusMessage = LocalizationService.Current.QueueNeedsItems;
            return;
        }

        var builder = new StringBuilder();
        var progress = new Progress<ProcessOutput>(line =>
        {
            if (!_settings.Current.KeepInstallLogs)
            {
                return;
            }

            builder.AppendLine(line.IsError ? "[err] " + line.Text : line.Text);
            InstallLog = builder.ToString();
        });

        try
        {
            IsInstalling = true;
            StatusMessage = LocalizationService.Current.QueueInstalling;
            await _queue.InstallAllAsync(_winget, silent, CancellationToken.None, progress);
            StatusMessage = LocalizationService.Current.QueueComplete;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsInstalling = false;
        }
    }

    public void MoveUp(QueueItem item)
    {
        _queue.Move(item, -1);
    }

    public void MoveDown(QueueItem item)
    {
        _queue.Move(item, 1);
    }

    public void Remove(QueueItem item)
    {
        _queue.Remove(item);
        OnPropertyChanged(nameof(HasItems));
    }

    public void Retry(QueueItem item)
    {
        _queue.ResetFailed(item);
    }

    public void SaveOrder()
    {
        _queue.Save();
    }

    public void Export(string path)
    {
        _queue.ExportToFile(path);
        StatusMessage = LocalizationService.Current.QueueExported(path);
    }

    public void Import(string path)
    {
        _queue.ImportFromFile(path);
        _queue.Save();
        StatusMessage = LocalizationService.Current.QueueImported(path);
        OnPropertyChanged(nameof(HasItems));
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasItems));
    }
}
