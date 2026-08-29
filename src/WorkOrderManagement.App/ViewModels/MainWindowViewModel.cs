using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WorkOrderManagement.App.ViewModels;

/// <summary>
/// ViewModel for MainWindow. Manages navigation between views.
/// </summary>
public partial class MainWindowViewModel : BaseViewModel
{
    private readonly WorkOrdersViewModel _workOrdersViewModel;
    private readonly TechniciansViewModel _techniciansViewModel;

    [ObservableProperty]
    private BaseViewModel _currentViewModel;

    [ObservableProperty]
    private bool _isWorkOrdersActive = true;

    [ObservableProperty]
    private bool _isTechniciansActive;

    public MainWindowViewModel(
        WorkOrdersViewModel workOrdersViewModel,
        TechniciansViewModel techniciansViewModel)
    {
        _workOrdersViewModel = workOrdersViewModel;
        _techniciansViewModel = techniciansViewModel;
        _currentViewModel = workOrdersViewModel;
    }

    [RelayCommand]
    private async Task NavigateToWorkOrdersAsync()
    {
        CurrentViewModel = _workOrdersViewModel;
        IsWorkOrdersActive = true;
        IsTechniciansActive = false;
        await _workOrdersViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task NavigateToTechniciansAsync()
    {
        CurrentViewModel = _techniciansViewModel;
        IsWorkOrdersActive = false;
        IsTechniciansActive = true;
        await _techniciansViewModel.LoadAsync();
    }

    /// <summary>Called by App.xaml.cs after the window is shown to load initial data.</summary>
    public async Task InitializeAsync()
        => await _workOrdersViewModel.LoadAsync();
}
