namespace WorkOrderManagement.Application.DTOs;

/// <summary>
/// Lightweight summary for dashboard statistics.
/// </summary>
public class WorkOrderSummary
{
    public int Total { get; init; }
    public int Open { get; init; }
    public int InProgress { get; init; }
    public int Completed { get; init; }
    public int HighPriority { get; init; }
    public int Overdue { get; init; }
}
