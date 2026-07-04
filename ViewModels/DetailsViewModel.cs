using System.Text;
using GetGUI.Infrastructure;
using GetGUI.Models;
using GetGUI.Services;

namespace GetGUI.ViewModels;

public sealed class DetailsViewModel : ObservableObject
{
    private readonly WingetService _winget;
    private readonly QueueService _queue;
    private readonly SettingsService _settings;
    private ApplicationPackage _application = new();
    private bool _isBusy;
    private string _statusMessage = string.Empty;
    private string _operationLog = string.Empty;

    public DetailsViewModel(WingetService winget, QueueService queue, SettingsService settings)
    {
        _winget = winget;
        _queue = queue;
        _settings = settings;
    }

    public ApplicationPackage Application
    {
        get => _application;
        private set => SetProperty(ref _application, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatus));
            }
        }
    }

    public bool HasStatus => !string.IsNullOrWhiteSpace(StatusMessage);

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

    public async Task LoadAsync(ApplicationPackage package)
    {
        Application = package.Clone();
        if (string.IsNullOrWhiteSpace(Application.Id))
        {
            StatusMessage = LocalizationService.Current.InvalidWingetApp;
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = LocalizationService.Current.LoadingDetails;
            var originalName = Application.Name;
            var details = await _winget.ShowAsync(Application.Id, CancellationToken.None, logOutput: true);
            if (!string.IsNullOrWhiteSpace(originalName) && LooksLikePackageId(details.Name, details.Id))
            {
                details.Name = originalName;
            }

            Application = details;
            StatusMessage = LocalizationService.Current.DetailsLoaded;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public bool AddToQueue()
    {
        var added = _queue.Add(Application);
        StatusMessage = added
            ? LocalizationService.Current.AddedToQueue(Application.Name)
            : LocalizationService.Current.AlreadyInQueue(Application.Name);
        return added;
    }

    public Task InstallAsync(bool silent)
    {
        return RunPackageOperationAsync(LocalizationService.Current.InstallOperation, token => _winget.InstallAsync(Application.Id, silent, token, CreateLogProgress()));
    }

    public Task UninstallAsync()
    {
        return RunPackageOperationAsync(LocalizationService.Current.UninstallOperation, token => _winget.UninstallAsync(Application.Id, token, CreateLogProgress()));
    }

    private async Task RunPackageOperationAsync(string label, Func<CancellationToken, Task<ProcessResult>> operation)
    {
        try
        {
            IsBusy = true;
            OperationLog = string.Empty;
            StatusMessage = LocalizationService.Current.OperationInProgress(label, Application.Name);
            var result = await operation(CancellationToken.None);
            StatusMessage = result.IsSuccess
                ? LocalizationService.Current.OperationDone(label)
                : LocalizationService.Current.OperationFailed(label);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private IProgress<ProcessOutput> CreateLogProgress()
    {
        var builder = new StringBuilder();
        return new Progress<ProcessOutput>(line =>
        {
            if (!_settings.Current.KeepInstallLogs)
            {
                return;
            }

            builder.AppendLine(line.IsError ? "[err] " + line.Text : line.Text);
            OperationLog = builder.ToString();
        });
    }

    private static bool LooksLikePackageId(string name, string id)
    {
        return string.Equals(name, id, StringComparison.OrdinalIgnoreCase)
            || (name.Contains('.', StringComparison.Ordinal) && !name.Contains(' ', StringComparison.Ordinal));
    }
}
