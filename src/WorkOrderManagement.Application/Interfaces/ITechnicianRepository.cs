using WorkOrderManagement.Domain.Entities;

namespace WorkOrderManagement.Application.Interfaces;

/// <summary>
/// Data access contract for the Technician aggregate.
/// </summary>
public interface ITechnicianRepository
{
    Task<IReadOnlyList<Technician>> GetAllAsync();
    Task<Technician?> GetByIdAsync(int id);
    Task AddAsync(Technician technician);
    Task UpdateAsync(Technician technician);
    Task DeleteAsync(Technician technician);
    Task<bool> HasWorkOrdersAsync(int technicianId);
}
