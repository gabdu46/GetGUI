using GetGUI.Services;
using GetGUI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GetGUI.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        ViewModel = new SettingsViewModel(AppServices.Settings);
        InitializeComponent();
        SelectTheme(AppServices.Settings.Current.Theme);
    }

    public SettingsViewModel ViewModel { get; }

    private void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeBox.SelectedItem is not ComboBoxItem { Tag: string tag })
        {
            return;
        }

        if (Enum.TryParse<AppTheme>(tag, out var theme))
        {
            ViewModel.Settings.Theme = theme;
            ViewModel.Save();
            if (WindowLocator.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ApplyTheme();
            }
        }
    }

    private void Setting_Toggled(object sender, RoutedEventArgs e)
    {
        ViewModel.Save();
    }

    private void SelectTheme(AppTheme theme)
    {
        var tag = theme.ToString();
        foreach (var item in ThemeBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase))
            {
                ThemeBox.SelectedItem = item;
                return;
            }
        }
    }
}
