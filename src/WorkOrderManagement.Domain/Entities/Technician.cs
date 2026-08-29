namespace WorkOrderManagement.Domain.Entities;

/// <summary>
/// Represents a maintenance technician who can be assigned to work orders.
/// </summary>
public class Technician
{
    public const int MaxFullNameLength = 100;
    public const int MaxSpecialtyLength = 100;

    public int Id { get; set; }

    /// <summary>Full name of the technician. Required.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Specialty/trade of the technician. Optional.</summary>
    public string? Specialty { get; set; }

    /// <summary>Navigation property: work orders assigned to this technician.</summary>
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
