using GetGUI.Services;
using GetGUI.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GetGUI.Views;

public sealed partial class ConsolePage : Page
{
    private readonly DispatcherQueueTimer _refreshTimer;

    public LocalizationService Loc => LocalizationService.Current;

    public ConsolePage()
    {
        ViewModel = new ConsoleViewModel(AppServices.Log);
        InitializeComponent();

        _refreshTimer = DispatcherQueue.CreateTimer();
        _refreshTimer.Interval = TimeSpan.FromSeconds(1);
        _refreshTimer.Tick += (_, _) => RefreshLog(scrollToEnd: true);
    }

    public ConsoleViewModel ViewModel { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        RefreshLog(scrollToEnd: true);
        _refreshTimer.Start();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _refreshTimer.Stop();
        base.OnNavigatedFrom(e);
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshLog(scrollToEnd: true);
    }

    private async void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = Loc.ClearLogsTitle,
            Content = Loc.ClearLogsContent,
            PrimaryButtonText = Loc.Clear,
            CloseButtonText = Loc.Cancel,
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            ViewModel.Clear();
            ScrollToEnd();
        }
    }

    private void RefreshLog(bool scrollToEnd)
    {
        ViewModel.Refresh();
        if (scrollToEnd)
        {
            ScrollToEnd();
        }
    }

    private void ScrollToEnd()
    {
        LogBox.SelectionStart = LogBox.Text.Length;
        LogBox.SelectionLength = 0;
    }
}
