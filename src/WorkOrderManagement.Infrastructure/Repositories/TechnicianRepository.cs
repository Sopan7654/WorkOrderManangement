using Microsoft.EntityFrameworkCore;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Infrastructure.Data;

namespace WorkOrderManagement.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ITechnicianRepository.
/// Data access only — no business rules.
/// </summary>
public class TechnicianRepository : ITechnicianRepository
{
    private readonly WorkOrderDbContext _context;

    public TechnicianRepository(WorkOrderDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Technician>> GetAllAsync()
        => await _context.Technicians
            .AsNoTracking()
            .OrderBy(t => t.FullName)
            .ToListAsync();

    public async Task<Technician?> GetByIdAsync(int id)
        => await _context.Technicians.FindAsync(id);

    public async Task AddAsync(Technician technician)
    {
        _context.Technicians.Add(technician);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Technician technician)
    {
        var existing = await _context.Technicians.FindAsync(technician.Id);
        if (existing is not null)
        {
            existing.FullName = technician.FullName;
            existing.Specialty = technician.Specialty;
            await _context.SaveChangesAsync();
        }
        else
        {
            _context.Technicians.Attach(technician);
            _context.Entry(technician).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Technician technician)
    {
        var existing = await _context.Technicians.FindAsync(technician.Id);
        if (existing is not null)
        {
            _context.Technicians.Remove(existing);
        }
        else
        {
            _context.Technicians.Remove(technician);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasWorkOrdersAsync(int technicianId)
        => await _context.WorkOrders.AnyAsync(w => w.AssignedTechnicianId == technicianId);
}
