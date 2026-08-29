using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WorkOrderManagement.Domain.Enums;

namespace WorkOrderManagement.App.Converters;

/// <summary>
/// Maps Priority enum values to color brushes for visual priority indicators.
/// </summary>
public class PriorityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            Priority.High => new SolidColorBrush(Color.FromRgb(239, 68, 68)),   // Red-500
            Priority.Medium => new SolidColorBrush(Color.FromRgb(245, 158, 11)), // Amber-500
            Priority.Low => new SolidColorBrush(Color.FromRgb(34, 197, 94)),    // Green-500
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
