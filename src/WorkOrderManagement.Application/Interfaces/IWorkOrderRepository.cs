using WorkOrderManagement.Application.DTOs;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Domain.Enums;

namespace WorkOrderManagement.Application.Interfaces;

/// <summary>
/// Data access contract for the WorkOrder aggregate, with filtering support.
/// </summary>
public interface IWorkOrderRepository
{
    Task<IReadOnlyList<WorkOrder>> GetAllAsync();
    Task<IReadOnlyList<WorkOrder>> GetFilteredAsync(WorkOrderFilter filter);
    Task<WorkOrder?> GetByIdAsync(int id);
    Task AddAsync(WorkOrder workOrder);
    Task UpdateAsync(WorkOrder workOrder);
    Task DeleteAsync(WorkOrder workOrder);
}
