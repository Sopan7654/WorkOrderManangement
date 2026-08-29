using Moq;
using WorkOrderManagement.Application;
using WorkOrderManagement.Application.DTOs;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Application.Services;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace WorkOrderManagement.Tests;

/// <summary>
/// Tests for work order filtering logic.
/// Verifies that status, priority, and combined filters work as expected.
/// </summary>
public class WorkOrderFilteringTests
{
    private readonly Mock<IWorkOrderRepository> _workOrderRepo = new();
    private readonly Mock<ITechnicianRepository> _technicianRepo = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly WorkOrderService _service;

    public WorkOrderFilteringTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(DateTime.Now);
        _dateTimeProvider.Setup(d => d.Today).Returns(DateTime.Today);

        _service = new WorkOrderService(
            _workOrderRepo.Object,
            _technicianRepo.Object,
            _dateTimeProvider.Object,
            NullLogger<WorkOrderService>.Instance);
    }

    [Fact]
    public async Task FilterByStatus_Open_ReturnsOnlyOpenWorkOrdersAsync()
    {
        // Arrange
        var filter = new WorkOrderFilter { Status = WorkOrderStatus.Open };
        var openOrders = new List<WorkOrder>
        {
            new() { Id = 1, Title = "Open WO", Status = WorkOrderStatus.Open, Priority = Priority.Medium, TargetCompletionDate = DateTime.Today.AddDays(7) }
        };
        _workOrderRepo.Setup(r => r.GetFilteredAsync(It.Is<WorkOrderFilter>(f => f.Status == WorkOrderStatus.Open)))
            .ReturnsAsync(openOrders.AsReadOnly());

        // Act
        var result = await _service.GetFilteredAsync(filter);

        // Assert
        Assert.Single(result);
        Assert.All(result, w => Assert.Equal(WorkOrderStatus.Open, w.Status));
    }

    [Fact]
    public async Task FilterByPriority_High_ReturnsOnlyHighPriorityAsync()
    {
        // Arrange
        var filter = new WorkOrderFilter { Priority = Priority.High };
        var highOrders = new List<WorkOrder>
        {
            new() { Id = 2, Title = "High Priority WO", Status = WorkOrderStatus.InProgress, Priority = Priority.High, TargetCompletionDate = DateTime.Today.AddDays(1) }
        };
        _workOrderRepo.Setup(r => r.GetFilteredAsync(It.Is<WorkOrderFilter>(f => f.Priority == Priority.High)))
            .ReturnsAsync(highOrders.AsReadOnly());

        // Act
        var result = await _service.GetFilteredAsync(filter);

        // Assert
        Assert.Single(result);
        Assert.All(result, w => Assert.Equal(Priority.High, w.Priority));
    }

    [Fact]
    public async Task FilterByStatusAndPriority_OpenAndHigh_ReturnsCombinedResultAsync()
    {
        // Arrange
        var filter = new WorkOrderFilter { Status = WorkOrderStatus.Open, Priority = Priority.High };
        var matchingOrders = new List<WorkOrder>
        {
            new() { Id = 3, Title = "High+Open WO", Status = WorkOrderStatus.Open, Priority = Priority.High, TargetCompletionDate = DateTime.Today.AddDays(1) }
        };
        _workOrderRepo.Setup(r => r.GetFilteredAsync(It.Is<WorkOrderFilter>(f =>
            f.Status == WorkOrderStatus.Open && f.Priority == Priority.High)))
            .ReturnsAsync(matchingOrders.AsReadOnly());

        // Act
        var result = await _service.GetFilteredAsync(filter);

        // Assert
        Assert.Single(result);
        Assert.Equal(WorkOrderStatus.Open, result[0].Status);
        Assert.Equal(Priority.High, result[0].Priority);
    }

    [Fact]
    public async Task FilterByStatus_InProgress_ReturnsInProgressOrdersAsync()
    {
        // Arrange
        var filter = new WorkOrderFilter { Status = WorkOrderStatus.InProgress };
        var inProgressOrders = new List<WorkOrder>
        {
            new() { Id = 4, Title = "In Progress WO", Status = WorkOrderStatus.InProgress, Priority = Priority.Low, TargetCompletionDate = DateTime.Today.AddDays(3) }
        };
        _workOrderRepo.Setup(r => r.GetFilteredAsync(It.Is<WorkOrderFilter>(f => f.Status == WorkOrderStatus.InProgress)))
            .ReturnsAsync(inProgressOrders.AsReadOnly());

        // Act
        var result = await _service.GetFilteredAsync(filter);

        // Assert
        Assert.Single(result);
        Assert.Equal(WorkOrderStatus.InProgress, result[0].Status);
    }

    [Fact]
    public async Task NoFilter_ReturnsAllWorkOrdersAsync()
    {
        // Arrange
        var filter = new WorkOrderFilter();
        var allOrders = new List<WorkOrder>
        {
            new() { Id = 1, Status = WorkOrderStatus.Open, Priority = Priority.Low, Title = "A", TargetCompletionDate = DateTime.Today },
            new() { Id = 2, Status = WorkOrderStatus.InProgress, Priority = Priority.High, Title = "B", TargetCompletionDate = DateTime.Today },
            new() { Id = 3, Status = WorkOrderStatus.Completed, Priority = Priority.Medium, Title = "C", TargetCompletionDate = DateTime.Today }
        };
        _workOrderRepo.Setup(r => r.GetFilteredAsync(It.IsAny<WorkOrderFilter>()))
            .ReturnsAsync(allOrders.AsReadOnly());

        // Act
        var result = await _service.GetFilteredAsync(filter);

        // Assert
        Assert.Equal(3, result.Count);
    }
}
