using GetGUI.Infrastructure;
using GetGUI.Services;

namespace GetGUI.ViewModels;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public AppSettings Settings => _settingsService.Current;

    public void Save()
    {
        _settingsService.Save();
    }
}
