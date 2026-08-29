using System.Windows;
using WorkOrderManagement.App.ViewModels;

namespace WorkOrderManagement.App.Views;

/// <summary>
/// Code-behind for MainWindow. Thin — all logic is in MainWindowViewModel.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
