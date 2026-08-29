using Microsoft.Extensions.Logging;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Domain.Entities;

namespace WorkOrderManagement.Application.Services;

/// <summary>
/// Implements business logic for technician management.
/// </summary>
public class TechnicianService : ITechnicianService
{
    private readonly ITechnicianRepository _technicianRepository;
    private readonly ILogger<TechnicianService> _logger;

    public TechnicianService(ITechnicianRepository technicianRepository, ILogger<TechnicianService> logger)
    {
        _technicianRepository = technicianRepository;
        _logger = logger;
    }

    public Task<IReadOnlyList<Technician>> GetAllAsync()
        => _technicianRepository.GetAllAsync();

    public Task<Technician?> GetByIdAsync(int id)
        => _technicianRepository.GetByIdAsync(id);

    public async Task<Technician> CreateAsync(Technician technician)
    {
        ValidateTechnician(technician);

        technician.FullName = technician.FullName.Trim();
        technician.Specialty = technician.Specialty?.Trim();

        await _technicianRepository.AddAsync(technician);
        _logger.LogInformation("Technician created: {FullName} (Id={Id})", technician.FullName, technician.Id);
        return technician;
    }

    public async Task UpdateAsync(Technician technician)
    {
        ValidateTechnician(technician);

        technician.FullName = technician.FullName.Trim();
        technician.Specialty = technician.Specialty?.Trim();

        await _technicianRepository.UpdateAsync(technician);
        _logger.LogInformation("Technician updated: {FullName} (Id={Id})", technician.FullName, technician.Id);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var technician = await _technicianRepository.GetByIdAsync(id);
        if (technician is null)
        {
            _logger.LogWarning("Attempt to delete non-existent technician Id={Id}", id);
            return (false, "Technician not found.");
        }

        bool hasWorkOrders = await _technicianRepository.HasWorkOrdersAsync(id);
        if (hasWorkOrders)
        {
            _logger.LogInformation("Deletion blocked for technician Id={Id}: has assigned work orders.", id);
            return (false, "This technician is assigned to existing work orders and cannot be deleted.");
        }

        await _technicianRepository.DeleteAsync(technician);
        _logger.LogInformation("Technician deleted: Id={Id}", id);
        return (true, null);
    }

    private static void ValidateTechnician(Technician technician)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(technician.FullName))
            errors.Add("Full name is required.");
        else if (technician.FullName.Trim().Length > Technician.MaxFullNameLength)
            errors.Add($"Full name must not exceed {Technician.MaxFullNameLength} characters.");

        if (technician.Specialty is not null && technician.Specialty.Length > Technician.MaxSpecialtyLength)
            errors.Add($"Specialty must not exceed {Technician.MaxSpecialtyLength} characters.");

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}
