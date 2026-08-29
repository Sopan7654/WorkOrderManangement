using Moq;
using WorkOrderManagement.Application;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Application.Services;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace WorkOrderManagement.Tests;

/// <summary>
/// Tests for technician assignment and validation logic.
/// </summary>
public class TechnicianAssignmentTests
{
    private readonly Mock<IWorkOrderRepository> _workOrderRepo = new();
    private readonly Mock<ITechnicianRepository> _technicianRepo = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly WorkOrderService _workOrderService;
    private readonly TechnicianService _technicianService;

    public TechnicianAssignmentTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(DateTime.Now);
        _dateTimeProvider.Setup(d => d.Today).Returns(DateTime.Today);
        _workOrderRepo.Setup(r => r.AddAsync(It.IsAny<WorkOrder>())).Returns(Task.CompletedTask);
        _technicianRepo.Setup(r => r.AddAsync(It.IsAny<Technician>())).Returns(Task.CompletedTask);
        _technicianRepo.Setup(r => r.UpdateAsync(It.IsAny<Technician>())).Returns(Task.CompletedTask);

        _workOrderService = new WorkOrderService(
            _workOrderRepo.Object,
            _technicianRepo.Object,
            _dateTimeProvider.Object,
            NullLogger<WorkOrderService>.Instance);

        _technicianService = new TechnicianService(
            _technicianRepo.Object,
            NullLogger<TechnicianService>.Instance);
    }

    [Fact]
    public async Task AssignExistingTechnician_Succeeds_WorkOrderHasTechnicianAsync()
    {
        // Arrange
        var technician = new Technician { Id = 1, FullName = "Alice Green", Specialty = "Electrical" };
        _technicianRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(technician);

        var workOrder = new WorkOrder
        {
            Title = "Fix wiring",
            Priority = Priority.Medium,
            Status = WorkOrderStatus.Open,
            AssignedTechnicianId = 1,
            TargetCompletionDate = DateTime.Today.AddDays(7)
        };

        // Act
        await _workOrderService.CreateAsync(workOrder);

        // Assert
        Assert.Equal(1, workOrder.AssignedTechnicianId);
    }

    [Fact]
    public async Task AssignNonExistentTechnician_ThrowsValidationExceptionAsync()
    {
        // Arrange
        _technicianRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Technician?)null);

        var workOrder = new WorkOrder
        {
            Title = "Fix plumbing",
            Priority = Priority.Low,
            Status = WorkOrderStatus.Open,
            AssignedTechnicianId = 999,   // Non-existent technician
            TargetCompletionDate = DateTime.Today.AddDays(14)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _workOrderService.CreateAsync(workOrder));
    }

    [Fact]
    public async Task CreateTechnician_ValidData_SucceedsAsync()
    {
        // Arrange
        var technician = new Technician { FullName = "Bob Jones", Specialty = "Plumbing" };

        // Act
        var result = await _technicianService.CreateAsync(technician);

        // Assert
        Assert.Equal("Bob Jones", result.FullName);
        Assert.Equal("Plumbing", result.Specialty);
        _technicianRepo.Verify(r => r.AddAsync(technician), Times.Once);
    }

    [Fact]
    public async Task DeleteTechnician_WithWorkOrders_FailsWithMessageAsync()
    {
        // Arrange
        var technician = new Technician { Id = 5, FullName = "Assigned Tech", Specialty = "HVAC" };
        _technicianRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(technician);
        _technicianRepo.Setup(r => r.HasWorkOrdersAsync(5)).ReturnsAsync(true);

        // Act
        var (success, message) = await _technicianService.DeleteAsync(5);

        // Assert
        Assert.False(success);
        Assert.NotNull(message);
        Assert.Contains("work orders", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteTechnician_WithoutWorkOrders_SucceedsAsync()
    {
        // Arrange
        var technician = new Technician { Id = 6, FullName = "Free Tech", Specialty = "Electrical" };
        _technicianRepo.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(technician);
        _technicianRepo.Setup(r => r.HasWorkOrdersAsync(6)).ReturnsAsync(false);
        _technicianRepo.Setup(r => r.DeleteAsync(technician)).Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _technicianService.DeleteAsync(6);

        // Assert
        Assert.True(success);
        Assert.Null(message);
        _technicianRepo.Verify(r => r.DeleteAsync(technician), Times.Once);
    }
}
