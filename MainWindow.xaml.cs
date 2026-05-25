using GetGUI.Services;
using GetGUI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GetGUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "GetGUI";
        Closed += MainWindow_Closed;
        ApplyTheme();
        ContentFrame.Navigate(typeof(SearchPage));
        ShellNavigation.SelectedItem = ShellNavigation.MenuItems[0];
    }

    public void ApplyTheme()
    {
        Root.RequestedTheme = AppServices.Settings.Current.Theme switch
        {
            AppTheme.Light => ElementTheme.Light,
            AppTheme.Dark => ElementTheme.Dark,
            _ => ElementTheme.Default
        };
    }

    private void ShellNavigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item || item.Tag is not string tag)
        {
            return;
        }

        var pageType = tag switch
        {
            "queue" => typeof(QueuePage),
            "console" => typeof(ConsolePage),
            "settings" => typeof(SettingsPage),
            _ => typeof(SearchPage)
        };

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        AppServices.Log.AppendInfo("GetGUI ferme.");
        if (AppServices.Settings.Current.ClearLogsOnExit)
        {
            AppServices.Log.Clear();
        }
    }
}
