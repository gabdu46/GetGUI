using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace GetGUI.Converters;

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isVisible = value is bool boolean && boolean;
        if (parameter is string text && text.Equals("invert", StringComparison.OrdinalIgnoreCase))
        {
            isVisible = !isVisible;
        }

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility.Visible;
    }
}
