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
            StatusMessage = "Application winget invalide.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Chargement des details winget...";
            var originalName = Application.Name;
            var details = await _winget.ShowAsync(Application.Id, CancellationToken.None, logOutput: true);
            if (!string.IsNullOrWhiteSpace(originalName) && LooksLikePackageId(details.Name, details.Id))
            {
                details.Name = originalName;
            }

            Application = details;
            StatusMessage = "Details charges depuis winget.";
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
            ? $"{Application.Name} ajoute a la file."
            : $"{Application.Name} est deja dans la file.";
        return added;
    }

    public Task InstallAsync(bool silent)
    {
        return RunPackageOperationAsync("Installation", token => _winget.InstallAsync(Application.Id, silent, token, CreateLogProgress()));
    }

    public Task UninstallAsync()
    {
        return RunPackageOperationAsync("Desinstallation", token => _winget.UninstallAsync(Application.Id, token, CreateLogProgress()));
    }

    private async Task RunPackageOperationAsync(string label, Func<CancellationToken, Task<ProcessResult>> operation)
    {
        try
        {
            IsBusy = true;
            OperationLog = string.Empty;
            StatusMessage = $"{label} de {Application.Name}...";
            var result = await operation(CancellationToken.None);
            StatusMessage = result.IsSuccess
                ? $"{label} terminee."
                : $"{label} en echec.";
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
