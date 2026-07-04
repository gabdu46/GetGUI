using GetGUI.Models;
using GetGUI.Services;
using GetGUI.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GetGUI.Views;

public sealed partial class SearchPage : Page
{
    private readonly DispatcherQueueTimer _searchTimer;

    public LocalizationService Loc => LocalizationService.Current;

    public SearchPage()
    {
        ViewModel = new SearchViewModel(
            AppServices.Winget,
            AppServices.Queue,
            AppServices.Settings,
            AppServices.PopularPackageCache);
        InitializeComponent();
        Loaded += SearchPage_Loaded;

        _searchTimer = DispatcherQueue.CreateTimer();
        _searchTimer.Interval = TimeSpan.FromMilliseconds(450);
        _searchTimer.Tick += SearchTimer_Tick;
    }

    public SearchViewModel ViewModel { get; }

    private async void SearchPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadPopularAsync();
    }

    private async void SearchTimer_Tick(DispatcherQueueTimer sender, object args)
    {
        _searchTimer.Stop();
        await ViewModel.SearchAsync(SearchBox.Text);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
        {
            return;
        }

        _searchTimer.Stop();
        _searchTimer.Start();
    }

    private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        _searchTimer.Stop();
        await ViewModel.SearchAsync(sender.Text);
    }

    private void ResultsGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ApplicationPackage package)
        {
            Frame.Navigate(typeof(DetailsPage), package);
        }
    }

    private void DetailsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: ApplicationPackage package })
        {
            Frame.Navigate(typeof(DetailsPage), package);
        }
    }

    private void AddToQueueButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: ApplicationPackage package })
        {
            ViewModel.AddToQueue(package);
        }
    }

    private async void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: ApplicationPackage package })
        {
            await ViewModel.InstallAsync(package, AppServices.Settings.Current.SilentInstall);
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _searchTimer.Stop();
        base.OnNavigatedFrom(e);
    }
}
