using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrderManagement.Domain.Entities;

namespace WorkOrderManagement.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for WorkOrder.
/// </summary>
public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Title)
            .IsRequired()
            .HasMaxLength(WorkOrder.MaxTitleLength);

        builder.Property(w => w.Description)
            .HasMaxLength(WorkOrder.MaxDescriptionLength);

        builder.Property(w => w.Priority)
            .IsRequired();

        builder.Property(w => w.Status)
            .IsRequired();

        builder.Property(w => w.DateLogged)
            .IsRequired();

        builder.Property(w => w.TargetCompletionDate)
            .IsRequired();

        builder.Property(w => w.AssignedTechnicianId)
            .IsRequired(false);

        // FK relationship is configured on the Technician side
    }
}
