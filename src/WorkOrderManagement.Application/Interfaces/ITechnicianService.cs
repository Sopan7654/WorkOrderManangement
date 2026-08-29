using WorkOrderManagement.Domain.Entities;

namespace WorkOrderManagement.Application.Interfaces;

/// <summary>
/// Application-level operations for Technician management.
/// Enforces business rules before delegating to the repository.
/// </summary>
public interface ITechnicianService
{
    Task<IReadOnlyList<Technician>> GetAllAsync();
    Task<Technician?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new technician after validating all rules.
    /// </summary>
    /// <returns>The created technician with its generated Id.</returns>
    Task<Technician> CreateAsync(Technician technician);

    /// <summary>Updates an existing technician.</summary>
    Task UpdateAsync(Technician technician);

    /// <summary>
    /// Attempts to delete a technician.
    /// </summary>
    /// <returns>
    /// (success: true, message: null) on success.
    /// (success: false, message: &lt;reason&gt;) when deletion is blocked.
    /// </returns>
    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
}
