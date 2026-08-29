using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Domain.Enums;

namespace WorkOrderManagement.App.ViewModels;

/// <summary>
/// ViewModel for the Add/Edit Work Order dialog.
/// Handles input binding, validation display, business rule communication,
/// and the overdue completion warning flow.
/// </summary>
public partial class AddEditWorkOrderViewModel : BaseViewModel
{
    private readonly IWorkOrderService _workOrderService;
    private readonly ILogger _logger;
    private readonly bool _isEditMode;
    private readonly int? _existingId;
    private readonly DateTime _dateLogged = DateTime.Now;

    // ── Form fields ───────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitleError))]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHighPriority))]
    [NotifyPropertyChangedFor(nameof(HighPriorityNote))]
    private Priority _selectedPriority = Priority.Medium;

    [ObservableProperty]
    private WorkOrderStatus _selectedStatus = WorkOrderStatus.Open;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TargetDateSummary))]
    private DateTime _targetCompletionDate = DateTime.Today.AddDays(7);

    [ObservableProperty]
    private Technician? _selectedTechnician;

    [ObservableProperty]
    private ObservableCollection<Technician?> _technicians = [];

    // ── Validation error messages ─────────────────────────────────────────────
    [ObservableProperty]
    private string _titleError = string.Empty;

    [ObservableProperty]
    private string _generalError = string.Empty;

    // ── Dialog result ─────────────────────────────────────────────────────────
    /// <summary>Set to true when the user confirms saving. The View reads this.</summary>
    public bool DialogResult { get; private set; }

    // ── Enum source lists ─────────────────────────────────────────────────────
    public IReadOnlyList<Priority> PriorityOptions { get; } =
        [Priority.Low, Priority.Medium, Priority.High];

    public IReadOnlyList<WorkOrderStatus> StatusOptions { get; } =
        [WorkOrderStatus.Open, WorkOrderStatus.InProgress, WorkOrderStatus.Completed];

    // ── Computed properties ───────────────────────────────────────────────────
    public bool IsHighPriority => SelectedPriority == Priority.High;

    public string HighPriorityNote =>
        IsHighPriority ? "High priority rule: completion date is automatically locked to tomorrow." : string.Empty;

    public string WindowTitle => _isEditMode ? "Edit Work Order" : "Add Work Order";

    public bool IsTargetDateEnabled => !IsHighPriority;

    public string TargetDateSummary
    {
        get
        {
            var diff = (TargetCompletionDate.Date - DateTime.Today).Days;
            var formatted = TargetCompletionDate.ToString("ddd, dd MMM yyyy");
            if (diff == 0) return $"Due Today ({formatted})";
            if (diff == 1) return $"Due Tomorrow ({formatted})";
            if (diff > 1) return $"Due in {diff} days ({formatted})";
            return $"Target date was {-diff} day(s) ago ({formatted})";
        }
    }

    public AddEditWorkOrderViewModel(
        WorkOrder? existingWorkOrder,
        IEnumerable<Technician> technicians,
        IWorkOrderService workOrderService,
        ILogger logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;

        var list = new List<Technician?> { null };
        list.AddRange(technicians.Where(t => t is not null));
        Technicians = new ObservableCollection<Technician?>(list);

        if (existingWorkOrder is not null)
        {
            _isEditMode = true;
            _existingId = existingWorkOrder.Id;
            _dateLogged = existingWorkOrder.DateLogged;
            Title = existingWorkOrder.Title;
            Description = existingWorkOrder.Description ?? string.Empty;
            SelectedPriority = existingWorkOrder.Priority;
            SelectedStatus = existingWorkOrder.Status;
            TargetCompletionDate = existingWorkOrder.TargetCompletionDate;
            SelectedTechnician = Technicians.FirstOrDefault(t => t?.Id == existingWorkOrder.AssignedTechnicianId);
        }
        else
        {
            _isEditMode = false;
            TargetCompletionDate = DateTime.Today.AddDays(7);
            SelectedTechnician = null;
        }
    }

    partial void OnSelectedPriorityChanged(Priority value)
    {
        OnPropertyChanged(nameof(IsTargetDateEnabled));
        if (value == Priority.High)
        {
            TargetCompletionDate = DateTime.Today.AddDays(1);
        }
        OnPropertyChanged(nameof(TargetDateSummary));
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void SetTargetDays(string daysText)
    {
        if (int.TryParse(daysText, out int days) && !IsHighPriority)
        {
            TargetCompletionDate = DateTime.Today.AddDays(days);
        }
    }

    [RelayCommand]
    private async Task SaveAsync(Window window)
    {
        if (!ValidateInput()) return;

        IsBusy = true;
        BusyMessage = _isEditMode ? "Saving changes..." : "Creating work order...";

        try
        {
            var workOrder = BuildWorkOrder();

            // Business Rule #2: Overdue completion check
            if (workOrder.Status == WorkOrderStatus.Completed)
            {
                bool isOverdue = _workOrderService.IsOverdue(workOrder);
                if (isOverdue)
                {
                    var confirm = MessageBox.Show(
                        "This work order is overdue. Its target completion date has passed.\n\n" +
                        "Are you sure you want to mark it as Completed?",
                        "Overdue Work Order",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (confirm == MessageBoxResult.No)
                    {
                        IsBusy = false;
                        return; // User chose not to complete it
                    }
                }
            }

            if (_isEditMode)
                await _workOrderService.UpdateAsync(workOrder);
            else
                await _workOrderService.CreateAsync(workOrder);

            DialogResult = true;
            window.DialogResult = true;
            window.Close();
        }
        catch (Application.ValidationException ex)
        {
            GeneralError = string.Join("\n", ex.Errors);
            _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", ex.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save work order.");
            GeneralError = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static void Cancel(Window window)
    {
        window.DialogResult = false;
        window.Close();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool ValidateInput()
    {
        bool isValid = true;
        TitleError = string.Empty;
        GeneralError = string.Empty;

        if (string.IsNullOrWhiteSpace(Title))
        {
            TitleError = "Title is required.";
            isValid = false;
        }
        else if (Title.Trim().Length > WorkOrder.MaxTitleLength)
        {
            TitleError = $"Title must not exceed {WorkOrder.MaxTitleLength} characters.";
            isValid = false;
        }

        return isValid;
    }

    private WorkOrder BuildWorkOrder()
    {
        var workOrder = new WorkOrder
        {
            Title = Title.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            Priority = SelectedPriority,
            Status = SelectedStatus,
            DateLogged = _dateLogged,
            TargetCompletionDate = TargetCompletionDate,
            AssignedTechnicianId = SelectedTechnician?.Id,
            AssignedTechnician = null
        };

        if (_isEditMode && _existingId.HasValue)
            workOrder.Id = _existingId.Value;

        return workOrder;
    }
}
