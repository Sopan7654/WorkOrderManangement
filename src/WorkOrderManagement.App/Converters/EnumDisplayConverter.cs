using System.Globalization;
using System.Windows.Data;
using WorkOrderManagement.Domain.Enums;

namespace WorkOrderManagement.App.Converters;

/// <summary>
/// Converts Priority and WorkOrderStatus enum values to human-readable display strings.
/// </summary>
public class EnumDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return parameter?.ToString() ?? "All";
        }

        return value switch
        {
            Priority.Low => "Low",
            Priority.Medium => "Medium",
            Priority.High => "High",
            WorkOrderStatus.Open => "Open",
            WorkOrderStatus.InProgress => "In Progress",
            WorkOrderStatus.Completed => "Completed",
            _ => value.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
