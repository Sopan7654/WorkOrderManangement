using WorkOrderManagement.Application.DTOs;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Domain.Enums;

namespace WorkOrderManagement.Application.Interfaces;

/// <summary>
/// Application-level operations for WorkOrder management.
/// Contains all business rules related to work orders.
/// </summary>
public interface IWorkOrderService
{
    Task<IReadOnlyList<WorkOrder>> GetAllAsync();
    Task<IReadOnlyList<WorkOrder>> GetFilteredAsync(WorkOrderFilter filter);
    Task<WorkOrder?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new work order, applying business rules (e.g. High Priority date rule).
    /// </summary>
    Task<WorkOrder> CreateAsync(WorkOrder workOrder);

    /// <summary>
    /// Updates an existing work order.
    /// If the priority changes to High, the target completion date is automatically updated.
    /// </summary>
    Task UpdateAsync(WorkOrder workOrder);

    Task DeleteAsync(int id);

    /// <summary>
    /// Determines whether the given work order would be overdue if completed now.
    /// Used by the ViewModel to decide whether to show a warning dialog.
    /// </summary>
    bool IsOverdue(WorkOrder workOrder);

    /// <summary>
    /// Calculates dashboard summary statistics.
    /// </summary>
    Task<WorkOrderSummary> GetSummaryAsync();
}
