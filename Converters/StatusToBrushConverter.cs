using GetGUI.Models;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace GetGUI.Converters;

public sealed class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            QueueItemStatus.Installing => new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
            QueueItemStatus.Success => new SolidColorBrush(Microsoft.UI.Colors.ForestGreen),
            QueueItemStatus.Failed => new SolidColorBrush(Microsoft.UI.Colors.Firebrick),
            _ => new SolidColorBrush(Microsoft.UI.Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
