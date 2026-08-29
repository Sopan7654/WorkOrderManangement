using System.Globalization;
using System.Windows.Data;

namespace WorkOrderManagement.App.Converters;

/// <summary>
/// Returns a configurable text when the bound value is null, otherwise returns the value as-is.
/// </summary>
public class NullToStringConverter : IValueConverter
{
    /// <summary>Text to display when the bound value is null. Defaults to "Unassigned".</summary>
    public string NullText { get; set; } = "Unassigned";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is null ? NullText : value.ToString() ?? NullText;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
