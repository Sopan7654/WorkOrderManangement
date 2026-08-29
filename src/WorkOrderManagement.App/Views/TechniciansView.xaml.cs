using System.Windows.Controls;

namespace WorkOrderManagement.App.Views;

/// <summary>
/// Code-behind for TechniciansView. Thin — all logic in TechniciansViewModel.
/// </summary>
public partial class TechniciansView : UserControl
{
    public TechniciansView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is ViewModels.TechniciansViewModel vm && vm.SelectedTechnician is not null)
        {
            if (vm.EditTechnicianCommand.CanExecute(null))
            {
                vm.EditTechnicianCommand.Execute(null);
            }
        }
    }
}
