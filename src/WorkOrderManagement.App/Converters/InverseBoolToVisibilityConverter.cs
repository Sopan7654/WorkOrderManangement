using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WorkOrderManagement.App.Converters;

/// <summary>
/// Returns Visibility.Visible when the bound boolean is false (inverse of BoolToVisibilityConverter).
/// Used for empty-state panels.
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Collapsed;
}
