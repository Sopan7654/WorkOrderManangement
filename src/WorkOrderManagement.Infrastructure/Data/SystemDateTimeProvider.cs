using WorkOrderManagement.Application.Interfaces;

namespace WorkOrderManagement.Infrastructure.Data;

/// <summary>
/// Production implementation that delegates to the system clock.
/// </summary>
public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
    public DateTime Today => DateTime.Today;
}
