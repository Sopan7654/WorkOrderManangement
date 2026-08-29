using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WorkOrderManagement.App.Converters;

/// <summary>
/// Returns Visibility.Visible when the bound boolean is true, Collapsed otherwise.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}
