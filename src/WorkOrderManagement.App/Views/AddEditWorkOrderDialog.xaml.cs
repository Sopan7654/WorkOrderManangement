using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Extensions.Logging;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.App.ViewModels;
using WorkOrderManagement.Domain.Entities;

namespace WorkOrderManagement.App.Views;

/// <summary>
/// Code-behind for AddEditWorkOrderDialog.
/// Creates its own ViewModel and sets it as DataContext.
/// </summary>
public partial class AddEditWorkOrderDialog : Window
{
    public AddEditWorkOrderDialog(
        WorkOrder? workOrder,
        IEnumerable<Technician> technicians,
        IWorkOrderService workOrderService,
        ITechnicianService technicianService,
        ILogger logger)
    {
        InitializeComponent();

        var vm = new AddEditWorkOrderViewModel(
            workOrder,
            technicians,
            workOrderService,
            logger);

        DataContext = vm;
    }
}
