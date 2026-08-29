using Microsoft.Extensions.Logging;
using WorkOrderManagement.Application.DTOs;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Domain.Enums;

namespace WorkOrderManagement.Application.Services;

/// <summary>
/// Implements all work order business logic including the two critical business rules:
/// 1. High priority → TargetCompletionDate = today + 1 day (on create).
/// 2. Overdue detection → exposed via IsOverdue() for the ViewModel to act on.
/// </summary>
public class WorkOrderService : IWorkOrderService
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<WorkOrderService> _logger;

    public WorkOrderService(
        IWorkOrderRepository workOrderRepository,
        ITechnicianRepository technicianRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<WorkOrderService> logger)
    {
        _workOrderRepository = workOrderRepository;
        _technicianRepository = technicianRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public Task<IReadOnlyList<WorkOrder>> GetAllAsync()
        => _workOrderRepository.GetAllAsync();

    public Task<IReadOnlyList<WorkOrder>> GetFilteredAsync(WorkOrderFilter filter)
        => _workOrderRepository.GetFilteredAsync(filter);

    public Task<WorkOrder?> GetByIdAsync(int id)
        => _workOrderRepository.GetByIdAsync(id);

    /// <summary>
    /// Creates a new work order.
    /// Business Rule #1: If Priority is High, TargetCompletionDate is forced to today + 1 day.
    /// </summary>
    public async Task<WorkOrder> CreateAsync(WorkOrder workOrder)
    {
        await ValidateWorkOrderAsync(workOrder);

        workOrder.Title = workOrder.Title.Trim();
        workOrder.DateLogged = _dateTimeProvider.Now;

        ApplyHighPriorityRule(workOrder);

        await _workOrderRepository.AddAsync(workOrder);
        _logger.LogInformation("Work order created: '{Title}' (Id={Id})", workOrder.Title, workOrder.Id);
        return workOrder;
    }

    /// <summary>
    /// Updates an existing work order.
    /// If priority was changed to High, the target completion date is updated automatically.
    /// </summary>
    public async Task UpdateAsync(WorkOrder workOrder)
    {
        await ValidateWorkOrderAsync(workOrder);

        workOrder.Title = workOrder.Title.Trim();
        ApplyHighPriorityRule(workOrder);

        await _workOrderRepository.UpdateAsync(workOrder);
        _logger.LogInformation("Work order updated: '{Title}' (Id={Id})", workOrder.Title, workOrder.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var workOrder = await _workOrderRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Work order with Id={id} was not found.");

        await _workOrderRepository.DeleteAsync(workOrder);
        _logger.LogInformation("Work order deleted: Id={Id}", id);
    }

    /// <summary>
    /// Business Rule #2 helper: returns true when the work order's target completion date
    /// is strictly before today's date. The ViewModel uses this to decide whether to show a
    /// confirmation dialog before marking the order as completed.
    /// </summary>
    public bool IsOverdue(WorkOrder workOrder)
        => workOrder.TargetCompletionDate.Date < _dateTimeProvider.Today.Date;

    public async Task<WorkOrderSummary> GetSummaryAsync()
    {
        var all = await _workOrderRepository.GetAllAsync();
        var today = _dateTimeProvider.Today;

        return new WorkOrderSummary
        {
            Total = all.Count,
            Open = all.Count(w => w.Status == WorkOrderStatus.Open),
            InProgress = all.Count(w => w.Status == WorkOrderStatus.InProgress),
            Completed = all.Count(w => w.Status == WorkOrderStatus.Completed),
            HighPriority = all.Count(w => w.Priority == Priority.High),
            Overdue = all.Count(w =>
                w.Status != WorkOrderStatus.Completed &&
                w.TargetCompletionDate.Date < today.Date)
        };
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Business Rule #1: Forces TargetCompletionDate to today + 1 day for High-priority orders.
    /// Applied on both Create and Update so the rule cannot be bypassed.
    /// </summary>
    private void ApplyHighPriorityRule(WorkOrder workOrder)
    {
        if (workOrder.Priority == Priority.High)
        {
            workOrder.TargetCompletionDate = _dateTimeProvider.Today.AddDays(1);
            _logger.LogDebug("High-priority rule applied: TargetCompletionDate set to {Date}", workOrder.TargetCompletionDate);
        }
    }

    private async Task ValidateWorkOrderAsync(WorkOrder workOrder)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(workOrder.Title))
            errors.Add("Title is required.");
        else if (workOrder.Title.Trim().Length > WorkOrder.MaxTitleLength)
            errors.Add($"Title must not exceed {WorkOrder.MaxTitleLength} characters.");

        if (workOrder.Description is not null && workOrder.Description.Length > WorkOrder.MaxDescriptionLength)
            errors.Add($"Description must not exceed {WorkOrder.MaxDescriptionLength} characters.");

        // Validate technician reference if supplied
        if (workOrder.AssignedTechnicianId.HasValue)
        {
            var technician = await _technicianRepository.GetByIdAsync(workOrder.AssignedTechnicianId.Value);
            if (technician is null)
                errors.Add("The assigned technician does not exist.");
        }

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}
