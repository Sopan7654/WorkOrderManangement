using WorkOrderManagement.Domain.Enums;

namespace WorkOrderManagement.Domain.Entities;

/// <summary>
/// Represents a maintenance work order.
/// </summary>
public class WorkOrder
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;

    public int Id { get; set; }

    /// <summary>Short descriptive title. Required.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Detailed description of the work. Optional.</summary>
    public string? Description { get; set; }

    /// <summary>Urgency level.</summary>
    public Priority Priority { get; set; } = Priority.Medium;

    /// <summary>Current lifecycle state.</summary>
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Open;

    /// <summary>When the work order was logged into the system.</summary>
    public DateTime DateLogged { get; set; } = DateTime.Now;

    /// <summary>Target date for completion. Business rule: High priority = today + 1 day.</summary>
    public DateTime TargetCompletionDate { get; set; }

    /// <summary>Foreign key to assigned technician. Nullable (unassigned).</summary>
    public int? AssignedTechnicianId { get; set; }

    /// <summary>Navigation property to the assigned technician.</summary>
    public Technician? AssignedTechnician { get; set; }
}
