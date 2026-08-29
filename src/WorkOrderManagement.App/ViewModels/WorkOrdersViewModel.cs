using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using WorkOrderManagement.Application.DTOs;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Domain.Entities;
using WorkOrderManagement.Domain.Enums;

namespace WorkOrderManagement.App.ViewModels;

public record FilterOption<T>(string Name, T? Value) where T : struct;

/// <summary>
/// ViewModel for the Work Orders view.
/// Coordinates filtering, CRUD operations, dashboard, and business rule confirmations.
/// </summary>
public partial class WorkOrdersViewModel : BaseViewModel
{
    private readonly IWorkOrderService _workOrderService;
    private readonly ITechnicianService _technicianService;
    private readonly ILogger<WorkOrdersViewModel> _logger;

    // ── Observable collections ────────────────────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<WorkOrder> _workOrders = [];

    [ObservableProperty]
    private WorkOrder? _selectedWorkOrder;

    [ObservableProperty]
    private ObservableCollection<Technician> _technicians = [];

    // ── Filter state ──────────────────────────────────────────────────────────
    [ObservableProperty]
    private FilterOption<WorkOrderStatus> _selectedStatusFilter;

    [ObservableProperty]
    private FilterOption<Priority> _selectedPriorityFilter;

    // ── Dashboard summary ─────────────────────────────────────────────────────
    [ObservableProperty]
    private WorkOrderSummary _summary = new();

    // ── Status feedback ───────────────────────────────────────────────────────
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // ── Filter source lists for ComboBoxes ────────────────────────────────────
    public IReadOnlyList<FilterOption<WorkOrderStatus>> StatusFilterOptions { get; } =
    [
        new("All Statuses", null),
        new("Open", WorkOrderStatus.Open),
        new("In Progress", WorkOrderStatus.InProgress),
        new("Completed", WorkOrderStatus.Completed)
    ];

    public IReadOnlyList<FilterOption<Priority>> PriorityFilterOptions { get; } =
    [
        new("All Priorities", null),
        new("Low", Priority.Low),
        new("Medium", Priority.Medium),
        new("High", Priority.High)
    ];

    public WorkOrdersViewModel(
        IWorkOrderService workOrderService,
        ITechnicianService technicianService,
        ILogger<WorkOrdersViewModel> logger)
    {
        _workOrderService = workOrderService;
        _technicianService = technicianService;
        _logger = logger;
        _selectedStatusFilter = StatusFilterOptions[0];
        _selectedPriorityFilter = PriorityFilterOptions[0];
    }

    partial void OnSelectedStatusFilterChanged(FilterOption<WorkOrderStatus> value)
    {
        _ = LoadAsync();
    }

    partial void OnSelectedPriorityFilterChanged(FilterOption<Priority> value)
    {
        _ = LoadAsync();
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        BusyMessage = "Loading work orders...";
        try
        {
            var filter = BuildFilter();
            var orders = await _workOrderService.GetFilteredAsync(filter);
            WorkOrders = new ObservableCollection<WorkOrder>(orders);

            var techs = await _technicianService.GetAllAsync();
            Technicians = new ObservableCollection<Technician>(techs);

            Summary = await _workOrderService.GetSummaryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load work orders.");
            ShowError("Failed to load work orders. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
        => await LoadAsync();

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        if (SelectedStatusFilter == StatusFilterOptions[0] && SelectedPriorityFilter == PriorityFilterOptions[0])
        {
            await LoadAsync();
            return;
        }
        SelectedStatusFilter = StatusFilterOptions[0];
        SelectedPriorityFilter = PriorityFilterOptions[0];
    }

    [RelayCommand]
    private async Task AddWorkOrderAsync()
    {
        var techs = await _technicianService.GetAllAsync();
        Technicians = new ObservableCollection<Technician>(techs);

        var dialog = new Views.AddEditWorkOrderDialog(
            workOrder: null,
            technicians: Technicians,
            workOrderService: _workOrderService,
            technicianService: _technicianService,
            logger: _logger);

        if (dialog.ShowDialog() == true)
        {
            await LoadAsync();
            ShowSuccess("Work order created successfully.");
        }
    }

    [RelayCommand]
    private async Task EditWorkOrderAsync()
    {
        if (SelectedWorkOrder is null) return;

        var techs = await _technicianService.GetAllAsync();
        Technicians = new ObservableCollection<Technician>(techs);

        var dialog = new Views.AddEditWorkOrderDialog(
            workOrder: SelectedWorkOrder,
            technicians: Technicians,
            workOrderService: _workOrderService,
            technicianService: _technicianService,
            logger: _logger);

        if (dialog.ShowDialog() == true)
        {
            await LoadAsync();
            ShowSuccess("Work order updated successfully.");
        }
    }

    [RelayCommand]
    private async Task DeleteWorkOrderAsync()
    {
        if (SelectedWorkOrder is null) return;

        var orderToDelete = SelectedWorkOrder;
        var result = MessageBox.Show(
            $"Are you sure you want to delete work order '{orderToDelete.Title}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            await _workOrderService.DeleteAsync(orderToDelete.Id);
            SelectedWorkOrder = null;
            await LoadAsync();
            ShowSuccess("Work order deleted.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete work order Id={Id}", orderToDelete.Id);
            ShowError("Failed to delete work order. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
        => await LoadAsync();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private WorkOrderFilter BuildFilter() => new()
    {
        Status = SelectedStatusFilter?.Value,
        Priority = SelectedPriorityFilter?.Value
    };

    private void ShowSuccess(string message)
    {
        StatusMessage = message;
        _logger.LogInformation(message);
    }

    private void ShowError(string message)
    {
        StatusMessage = message;
    }

    // ── Computed properties ───────────────────────────────────────────────────

    public bool HasWorkOrders => WorkOrders.Count > 0;
    public bool IsWorkOrderSelected => SelectedWorkOrder is not null;

    partial void OnWorkOrdersChanged(ObservableCollection<WorkOrder> value)
    {
        OnPropertyChanged(nameof(HasWorkOrders));
    }

    partial void OnSelectedWorkOrderChanged(WorkOrder? value)
    {
        OnPropertyChanged(nameof(IsWorkOrderSelected));
    }
}
