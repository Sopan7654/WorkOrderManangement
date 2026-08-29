using Microsoft.EntityFrameworkCore;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Infrastructure.Configurations;

namespace WorkOrderManagement.Infrastructure.Data;

/// <summary>
/// EF Core database context for the Work Order Management System.
/// </summary>
public class WorkOrderDbContext : DbContext
{
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public WorkOrderDbContext(DbContextOptions<WorkOrderDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TechnicianConfiguration());
        modelBuilder.ApplyConfiguration(new WorkOrderConfiguration());
    }
}
