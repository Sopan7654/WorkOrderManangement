namespace WorkOrderManagement.Application.Interfaces;

/// <summary>
/// Provides the current date/time, allowing deterministic testing through substitution.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>Returns the current local date and time.</summary>
    DateTime Now { get; }

    /// <summary>Returns today's local date (time component set to midnight).</summary>
    DateTime Today { get; }
}
