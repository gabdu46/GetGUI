using GetGUI.Infrastructure;

namespace GetGUI.Services;

public enum AppTheme
{
    Default,
    Light,
    Dark
}

public sealed class AppSettings : ObservableObject
{
    private bool _silentInstall = true;
    private bool _keepInstallLogs = true;
    private bool _clearLogsOnExit;
    private AppTheme _theme = AppTheme.Default;

    public bool SilentInstall
    {
        get => _silentInstall;
        set => SetProperty(ref _silentInstall, value);
    }

    public bool KeepInstallLogs
    {
        get => _keepInstallLogs;
        set => SetProperty(ref _keepInstallLogs, value);
    }

    public bool ClearLogsOnExit
    {
        get => _clearLogsOnExit;
        set => SetProperty(ref _clearLogsOnExit, value);
    }

    public AppTheme Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }
}
