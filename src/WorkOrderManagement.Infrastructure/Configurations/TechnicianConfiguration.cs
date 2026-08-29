using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderManagement.Domain.Entities;

namespace WorkOrderManagement.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for Technician.
/// </summary>
public class TechnicianConfiguration : IEntityTypeConfiguration<Technician>
{
    public void Configure(EntityTypeBuilder<Technician> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.FullName)
            .IsRequired()
            .HasMaxLength(Technician.MaxFullNameLength);

        builder.Property(t => t.Specialty)
            .HasMaxLength(Technician.MaxSpecialtyLength);

        // Relationship configured on WorkOrder side
        builder.HasMany(t => t.WorkOrders)
            .WithOne(w => w.AssignedTechnician)
            .HasForeignKey(w => w.AssignedTechnicianId)
            .OnDelete(DeleteBehavior.Restrict)  // Prevent accidental cascade-delete of work orders
            .IsRequired(false);
    }
}
