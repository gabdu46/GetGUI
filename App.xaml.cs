using GetGUI.Services;
using Microsoft.UI.Xaml;

namespace GetGUI;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        AppServices.Initialize();

        _window = new MainWindow();
        WindowLocator.MainWindow = _window;
        _window.Activate();
    }
}
