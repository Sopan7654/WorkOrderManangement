using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WorkOrderManagement.Domain.Enums;

namespace WorkOrderManagement.App.Converters;

/// <summary>
/// Maps WorkOrderStatus enum values to color brushes for status badges.
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            WorkOrderStatus.Open => new SolidColorBrush(Color.FromRgb(59, 130, 246)),      // Blue-500
            WorkOrderStatus.InProgress => new SolidColorBrush(Color.FromRgb(168, 85, 247)), // Purple-500
            WorkOrderStatus.Completed => new SolidColorBrush(Color.FromRgb(34, 197, 94)),   // Green-500
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
