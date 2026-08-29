using Moq;
using WorkOrderManagement.Application;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Application.Services;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace WorkOrderManagement.Tests;

/// <summary>
/// Tests for work order and technician validation rules.
/// </summary>
public class ValidationTests
{
    private readonly Mock<IWorkOrderRepository> _workOrderRepo = new();
    private readonly Mock<ITechnicianRepository> _technicianRepo = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly WorkOrderService _workOrderService;
    private readonly TechnicianService _technicianService;

    public ValidationTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(DateTime.Now);
        _dateTimeProvider.Setup(d => d.Today).Returns(DateTime.Today);
        _technicianRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Technician?)null);
        _technicianRepo.Setup(r => r.AddAsync(It.IsAny<Technician>())).Returns(Task.CompletedTask);

        _workOrderService = new WorkOrderService(
            _workOrderRepo.Object,
            _technicianRepo.Object,
            _dateTimeProvider.Object,
            NullLogger<WorkOrderService>.Instance);

        _technicianService = new TechnicianService(
            _technicianRepo.Object,
            NullLogger<TechnicianService>.Instance);
    }

    // ── Work Order Validation ─────────────────────────────────────────────

    [Fact]
    public async Task CreateWorkOrder_EmptyTitle_ThrowsValidationExceptionAsync()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            Title = "",   // Missing required title
            Priority = Priority.Medium,
            Status = WorkOrderStatus.Open,
            TargetCompletionDate = DateTime.Today.AddDays(7)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _workOrderService.CreateAsync(workOrder));
    }

    [Fact]
    public async Task CreateWorkOrder_WhiteSpaceTitle_ThrowsValidationExceptionAsync()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            Title = "   ",   // Whitespace only
            Priority = Priority.Low,
            Status = WorkOrderStatus.Open,
            TargetCompletionDate = DateTime.Today.AddDays(7)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _workOrderService.CreateAsync(workOrder));
    }

    [Fact]
    public async Task CreateWorkOrder_TitleTooLong_ThrowsValidationExceptionAsync()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            Title = new string('A', WorkOrder.MaxTitleLength + 1),   // Exceeds max length
            Priority = Priority.Medium,
            Status = WorkOrderStatus.Open,
            TargetCompletionDate = DateTime.Today.AddDays(7)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _workOrderService.CreateAsync(workOrder));
    }

    [Fact]
    public async Task CreateWorkOrder_ValidTitle_DoesNotThrowAsync()
    {
        // Arrange
        _workOrderRepo.Setup(r => r.AddAsync(It.IsAny<WorkOrder>())).Returns(Task.CompletedTask);
        var workOrder = new WorkOrder
        {
            Title = "Valid Work Order",
            Priority = Priority.Low,
            Status = WorkOrderStatus.Open,
            TargetCompletionDate = DateTime.Today.AddDays(7)
        };

        // Act & Assert — no exception
        var result = await _workOrderService.CreateAsync(workOrder);
        Assert.Equal("Valid Work Order", result.Title);
    }

    // ── Technician Validation ─────────────────────────────────────────────

    [Fact]
    public async Task CreateTechnician_EmptyFullName_ThrowsValidationExceptionAsync()
    {
        // Arrange
        var technician = new Technician { FullName = "" };  // Required field empty

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _technicianService.CreateAsync(technician));
    }

    [Fact]
    public async Task CreateTechnician_WhiteSpaceFullName_ThrowsValidationExceptionAsync()
    {
        // Arrange
        var technician = new Technician { FullName = "   " };  // Whitespace only

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _technicianService.CreateAsync(technician));
    }

    [Fact]
    public async Task CreateTechnician_FullNameTooLong_ThrowsValidationExceptionAsync()
    {
        // Arrange
        var technician = new Technician { FullName = new string('X', Technician.MaxFullNameLength + 1) };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _technicianService.CreateAsync(technician));
    }

    [Fact]
    public async Task CreateTechnician_SpecialtyTooLong_ThrowsValidationExceptionAsync()
    {
        // Arrange
        var technician = new Technician
        {
            FullName = "Valid Name",
            Specialty = new string('Z', Technician.MaxSpecialtyLength + 1)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _technicianService.CreateAsync(technician));
    }

    [Fact]
    public async Task CreateTechnician_ValidData_TrimsNameAsync()
    {
        // Arrange
        var technician = new Technician { FullName = "  Alice  ", Specialty = "  HVAC  " };

        // Act
        await _technicianService.CreateAsync(technician);

        // Assert — names should be trimmed by the service
        Assert.Equal("Alice", technician.FullName);
        Assert.Equal("HVAC", technician.Specialty);
    }
}
