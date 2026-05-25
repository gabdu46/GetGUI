using GetGUI.Models;
using GetGUI.Services;
using GetGUI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace GetGUI.Views;

public sealed partial class QueuePage : Page
{
    public QueuePage()
    {
        ViewModel = new QueueViewModel(AppServices.Queue, AppServices.Winget, AppServices.Settings);
        InitializeComponent();
    }

    public QueueViewModel ViewModel { get; }

    private async void InstallAllButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.InstallAllAsync(AppServices.Settings.Current.SilentInstall);
    }

    private void MoveUpButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: QueueItem item })
        {
            ViewModel.MoveUp(item);
        }
    }

    private void MoveDownButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: QueueItem item })
        {
            ViewModel.MoveDown(item);
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: QueueItem item })
        {
            ViewModel.Remove(item);
        }
    }

    private void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: QueueItem item })
        {
            ViewModel.Retry(item);
        }
    }

    private void QueueList_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        ViewModel.SaveOrder();
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileSavePicker
        {
            SuggestedFileName = "getgui-queue"
        };
        picker.FileTypeChoices.Add("JSON", new List<string> { ".json" });
        InitializePicker(picker);

        var file = await picker.PickSaveFileAsync();
        if (file is not null)
        {
            ViewModel.Export(file.Path);
        }
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".json");
        InitializePicker(picker);

        var file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            ViewModel.Import(file.Path);
        }
    }

    private static void InitializePicker(object picker)
    {
        if (WindowLocator.MainWindow is null)
        {
            return;
        }

        var handle = WindowNative.GetWindowHandle(WindowLocator.MainWindow);
        InitializeWithWindow.Initialize(picker, handle);
    }
}
