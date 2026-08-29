using Moq;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Application.Services;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace WorkOrderManagement.Tests;

/// <summary>
/// Tests for high-priority date rule (Business Rule #1).
/// Verifies that High priority work orders always get TargetCompletionDate = today + 1 day.
/// </summary>
public class HighPriorityRuleTests
{
    private readonly Mock<IWorkOrderRepository> _workOrderRepo = new();
    private readonly Mock<ITechnicianRepository> _technicianRepo = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly WorkOrderService _service;

    private static readonly DateTime FakeToday = new(2026, 8, 25);

    public HighPriorityRuleTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(FakeToday);
        _dateTimeProvider.Setup(d => d.Today).Returns(FakeToday);
        _workOrderRepo.Setup(r => r.AddAsync(It.IsAny<WorkOrder>())).Returns(Task.CompletedTask);
        _workOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkOrder>())).Returns(Task.CompletedTask);

        _service = new WorkOrderService(
            _workOrderRepo.Object,
            _technicianRepo.Object,
            _dateTimeProvider.Object,
            NullLogger<WorkOrderService>.Instance);
    }

    [Fact]
    public async Task CreateWorkOrder_HighPriority_SetsTargetDateToTomorrowAsync()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            Title = "Urgent Electrical Fault",
            Priority = Priority.High,
            Status = WorkOrderStatus.Open,
            TargetCompletionDate = DateTime.Today.AddDays(30) // User-supplied date should be overridden
        };

        // Act
        await _service.CreateAsync(workOrder);

        // Assert — TargetCompletionDate must be FakeToday + 1 day
        Assert.Equal(FakeToday.AddDays(1), workOrder.TargetCompletionDate);
    }

    [Fact]
    public async Task CreateWorkOrder_MediumPriority_DoesNotOverrideDateAsync()
    {
        // Arrange
        var targetDate = FakeToday.AddDays(14);
        var workOrder = new WorkOrder
        {
            Title = "Routine Maintenance",
            Priority = Priority.Medium,
            Status = WorkOrderStatus.Open,
            TargetCompletionDate = targetDate
        };

        // Act
        await _service.CreateAsync(workOrder);

        // Assert — date should NOT be changed for non-high priority
        Assert.Equal(targetDate, workOrder.TargetCompletionDate);
    }

    [Fact]
    public async Task CreateWorkOrder_LowPriority_DoesNotOverrideDateAsync()
    {
        // Arrange
        var targetDate = FakeToday.AddDays(30);
        var workOrder = new WorkOrder
        {
            Title = "Painting",
            Priority = Priority.Low,
            Status = WorkOrderStatus.Open,
            TargetCompletionDate = targetDate
        };

        // Act
        await _service.CreateAsync(workOrder);

        // Assert
        Assert.Equal(targetDate, workOrder.TargetCompletionDate);
    }

    [Fact]
    public async Task UpdateWorkOrder_ChangedToHighPriority_SetsTargetDateToTomorrowAsync()
    {
        // Arrange — simulating a work order being upgraded to High priority during update
        var workOrder = new WorkOrder
        {
            Id = 1,
            Title = "Escalated Fault",
            Priority = Priority.High,  // Changed to High
            Status = WorkOrderStatus.InProgress,
            TargetCompletionDate = FakeToday.AddDays(7) // Old date
        };

        // Act
        await _service.UpdateAsync(workOrder);

        // Assert — rule should also apply on update
        Assert.Equal(FakeToday.AddDays(1), workOrder.TargetCompletionDate);
    }
}
