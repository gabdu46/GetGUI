using GetGUI.Models;
using GetGUI.Services;
using GetGUI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GetGUI.Views;

public sealed partial class DetailsPage : Page
{
    public LocalizationService Loc => LocalizationService.Current;

    public DetailsPage()
    {
        ViewModel = new DetailsViewModel(AppServices.Winget, AppServices.Queue, AppServices.Settings);
        InitializeComponent();
    }

    public DetailsViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is ApplicationPackage package)
        {
            await ViewModel.LoadAsync(package);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private async void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.InstallAsync(AppServices.Settings.Current.SilentInstall);
    }

    private void AddToQueueButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddToQueue();
    }

    private async void UninstallButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = Loc.UninstallDialogTitle,
            Content = ViewModel.Application.Name,
            PrimaryButtonText = Loc.UninstallDialogPrimary,
            CloseButtonText = Loc.Cancel,
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.UninstallAsync();
        }
    }
}
