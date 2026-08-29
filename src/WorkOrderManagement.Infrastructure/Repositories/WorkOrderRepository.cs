using Microsoft.EntityFrameworkCore;
using WorkOrderManagement.Application.DTOs;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Infrastructure.Data;

namespace WorkOrderManagement.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IWorkOrderRepository.
/// Data access only — no business rules.
/// </summary>
public class WorkOrderRepository : IWorkOrderRepository
{
    private readonly WorkOrderDbContext _context;

    public WorkOrderRepository(WorkOrderDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WorkOrder>> GetAllAsync()
        => await _context.WorkOrders
            .AsNoTracking()
            .Include(w => w.AssignedTechnician)
            .OrderByDescending(w => w.DateLogged)
            .ToListAsync();

    public async Task<IReadOnlyList<WorkOrder>> GetFilteredAsync(WorkOrderFilter filter)
    {
        IQueryable<WorkOrder> query = _context.WorkOrders
            .AsNoTracking()
            .Include(w => w.AssignedTechnician);

        if (filter.Status.HasValue)
            query = query.Where(w => w.Status == filter.Status.Value);

        if (filter.Priority.HasValue)
            query = query.Where(w => w.Priority == filter.Priority.Value);

        return await query
            .OrderByDescending(w => w.DateLogged)
            .ToListAsync();
    }

    public async Task<WorkOrder?> GetByIdAsync(int id)
        => await _context.WorkOrders
            .Include(w => w.AssignedTechnician)
            .FirstOrDefaultAsync(w => w.Id == id);

    public async Task AddAsync(WorkOrder workOrder)
    {
        workOrder.AssignedTechnician = null;
        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(WorkOrder workOrder)
    {
        var existing = await _context.WorkOrders.FindAsync(workOrder.Id);
        if (existing is not null)
        {
            existing.Title = workOrder.Title;
            existing.Description = workOrder.Description;
            existing.Priority = workOrder.Priority;
            existing.Status = workOrder.Status;
            existing.TargetCompletionDate = workOrder.TargetCompletionDate;
            existing.AssignedTechnicianId = workOrder.AssignedTechnicianId;
            await _context.SaveChangesAsync();
        }
        else
        {
            workOrder.AssignedTechnician = null;
            _context.WorkOrders.Attach(workOrder);
            _context.Entry(workOrder).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(WorkOrder workOrder)
    {
        var existing = await _context.WorkOrders.FindAsync(workOrder.Id);
        if (existing is not null)
        {
            _context.WorkOrders.Remove(existing);
        }
        else
        {
            _context.WorkOrders.Remove(workOrder);
        }
        await _context.SaveChangesAsync();
    }
}
