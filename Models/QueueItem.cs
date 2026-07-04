using GetGUI.Infrastructure;
using GetGUI.Services;

namespace GetGUI.Models;

public sealed class QueueItem : ObservableObject
{
    private QueueItemStatus _status;
    private string _lastMessage = string.Empty;

    public QueueItem(ApplicationPackage application)
    {
        Application = application;
    }

    public ApplicationPackage Application { get; }

    public QueueItemStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public string LastMessage
    {
        get => _lastMessage;
        set => SetProperty(ref _lastMessage, value);
    }

    public string StatusText => Status switch
    {
        QueueItemStatus.Pending => LocalizationService.Current.StatusPending,
        QueueItemStatus.Installing => LocalizationService.Current.StatusInstalling,
        QueueItemStatus.Success => LocalizationService.Current.StatusSuccess,
        QueueItemStatus.Failed => LocalizationService.Current.StatusFailed,
        _ => LocalizationService.Current.StatusUnknown
    };
}
