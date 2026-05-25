using GetGUI.Infrastructure;
using GetGUI.Services;

namespace GetGUI.ViewModels;

public sealed class ConsoleViewModel : ObservableObject
{
    private readonly AppLogService _logService;
    private string _logText = string.Empty;

    public ConsoleViewModel(AppLogService logService)
    {
        _logService = logService;
        Refresh();
    }

    public string LogText
    {
        get => _logText;
        private set => SetProperty(ref _logText, value);
    }

    public string LogPath => _logService.LogPath;

    public void Refresh()
    {
        LogText = _logService.Text;
    }

    public void Clear()
    {
        _logService.Clear();
        Refresh();
    }
}
