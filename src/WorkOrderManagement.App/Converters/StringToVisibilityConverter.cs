using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WorkOrderManagement.App.Converters;

/// <summary>
/// Returns Visibility.Visible when the bound value is a non-empty string, Collapsed otherwise.
/// Used to show/hide validation error messages.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s)
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
