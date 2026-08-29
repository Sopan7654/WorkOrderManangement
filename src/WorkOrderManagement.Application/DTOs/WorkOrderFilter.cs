using WorkOrderManagement.Domain.Enums;

namespace WorkOrderManagement.Application.DTOs;

/// <summary>
/// Carries optional filter criteria for work order queries.
/// A null property means "no filter on that dimension".
/// </summary>
public class WorkOrderFilter
{
    public WorkOrderStatus? Status { get; set; }
    public Priority? Priority { get; set; }

    /// <summary>Returns true when no filter criteria have been set.</summary>
    public bool IsEmpty => Status is null && Priority is null;
}
