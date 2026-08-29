using System.Windows.Controls;
using WorkOrderManagement.App.ViewModels;

namespace WorkOrderManagement.App.Views;

/// <summary>
/// Code-behind for WorkOrdersView. Thin — triggers filter load from ComboBox selection events.
/// </summary>
public partial class WorkOrdersView : UserControl
{
    public WorkOrdersView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is WorkOrdersViewModel vm && vm.SelectedWorkOrder is not null)
        {
            if (vm.EditWorkOrderCommand.CanExecute(null))
            {
                vm.EditWorkOrderCommand.Execute(null);
            }
        }
    }
}
