using System.Windows;
using Microsoft.Extensions.Logging;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.App.ViewModels;
using WorkOrderManagement.Domain.Entities;

namespace WorkOrderManagement.App.Views;

/// <summary>
/// Code-behind for AddEditTechnicianDialog. Creates ViewModel and sets DataContext.
/// </summary>
public partial class AddEditTechnicianDialog : Window
{
    public AddEditTechnicianDialog(
        Technician? technician,
        ITechnicianService technicianService,
        ILogger logger)
    {
        InitializeComponent();
        DataContext = new AddEditTechnicianViewModel(technician, technicianService, logger);
    }
}
