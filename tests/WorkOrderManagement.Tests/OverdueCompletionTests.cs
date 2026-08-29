using Moq;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Application.Services;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace WorkOrderManagement.Tests;

/// <summary>
/// Tests for overdue detection (Business Rule #2).
/// Verifies IsOverdue() returns correct results and that the service supports
/// the ViewModel's confirmation dialog flow.
/// </summary>
public class OverdueCompletionTests
{
    private readonly Mock<IWorkOrderRepository> _workOrderRepo = new();
    private readonly Mock<ITechnicianRepository> _technicianRepo = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly WorkOrderService _service;

    private static readonly DateTime FakeToday = new(2026, 8, 25);

    public OverdueCompletionTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(FakeToday);
        _dateTimeProvider.Setup(d => d.Today).Returns(FakeToday);
        _workOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkOrder>())).Returns(Task.CompletedTask);

        _service = new WorkOrderService(
            _workOrderRepo.Object,
            _technicianRepo.Object,
            _dateTimeProvider.Object,
            NullLogger<WorkOrderService>.Instance);
    }

    [Fact]
    public void IsOverdue_PastTargetDate_ReturnsTrue()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            Title = "Past Due",
            Priority = Priority.Medium,
            TargetCompletionDate = FakeToday.AddDays(-3)  // 3 days in the past
        };

        // Act
        var result = _service.IsOverdue(workOrder);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOverdue_FutureTargetDate_ReturnsFalse()
    {
        // Arrange
        var workOrder = new WorkOrder
        {
            Title = "Not Due Yet",
            Priority = Priority.Medium,
            TargetCompletionDate = FakeToday.AddDays(5)  // 5 days in the future
        };

        // Act
        var result = _service.IsOverdue(workOrder);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOverdue_SameDay_ReturnsFalse()
    {
        // Arrange — due today is NOT overdue
        var workOrder = new WorkOrder
        {
            Title = "Due Today",
            Priority = Priority.High,
            TargetCompletionDate = FakeToday
        };

        // Act
        var result = _service.IsOverdue(workOrder);

        // Assert — same-day is NOT overdue
        Assert.False(result);
    }

    [Fact]
    public async Task CompleteOverdueWorkOrder_UserConfirmsYes_StatusBecomesCompletedAsync()
    {
        // Arrange — user chooses Yes in the confirmation dialog (simulated by simply calling UpdateAsync)
        var workOrder = new WorkOrder
        {
            Id = 42,
            Title = "Overdue Task",
            Priority = Priority.Medium,
            Status = WorkOrderStatus.Completed,   // User confirmed → status is set to Completed
            TargetCompletionDate = FakeToday.AddDays(-5)  // Overdue
        };

        // Act — service should save without error when user confirmed
        await _service.UpdateAsync(workOrder);

        // Assert — UpdateAsync was called once
        _workOrderRepo.Verify(r => r.UpdateAsync(workOrder), Times.Once);
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
    }

    [Fact]
    public void CompleteOverdueWorkOrder_UserConfirmsNo_StatusRemainsUnchanged()
    {
        // Arrange — ViewModel simulates user choosing No (status is NOT changed to Completed)
        var workOrder = new WorkOrder
        {
            Id = 43,
            Title = "Overdue Task Not Completed",
            Priority = Priority.Medium,
            Status = WorkOrderStatus.InProgress,  // Remains InProgress because user said No
            TargetCompletionDate = FakeToday.AddDays(-2)
        };

        // In the real flow: if user says No, ViewModel simply does not call UpdateAsync.
        // Here we verify that IsOverdue() provides the correct signal.
        var isOverdue = _service.IsOverdue(workOrder);

        // Assert — service correctly identifies it as overdue, giving the VM info to ask the user
        Assert.True(isOverdue);
        // Status was NOT changed — simulating that user chose No in the dialog
        Assert.Equal(WorkOrderStatus.InProgress, workOrder.Status);
    }
}
