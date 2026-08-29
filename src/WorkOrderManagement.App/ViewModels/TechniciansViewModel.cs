using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Domain.Entities;

namespace WorkOrderManagement.App.ViewModels;

/// <summary>
/// ViewModel for the Technicians management view.
/// </summary>
public partial class TechniciansViewModel : BaseViewModel
{
    private readonly ITechnicianService _technicianService;
    private readonly ILogger<TechniciansViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<Technician> _technicians = [];

    [ObservableProperty]
    private Technician? _selectedTechnician;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public TechniciansViewModel(ITechnicianService technicianService, ILogger<TechniciansViewModel> logger)
    {
        _technicianService = technicianService;
        _logger = logger;
    }

    public bool IsTechnicianSelected => SelectedTechnician is not null;
    public bool HasTechnicians => Technicians.Count > 0;

    partial void OnSelectedTechnicianChanged(Technician? value)
        => OnPropertyChanged(nameof(IsTechnicianSelected));

    partial void OnTechniciansChanged(ObservableCollection<Technician> value)
        => OnPropertyChanged(nameof(HasTechnicians));

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        BusyMessage = "Loading technicians...";
        try
        {
            var techs = await _technicianService.GetAllAsync();
            Technicians = new ObservableCollection<Technician>(techs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load technicians.");
            StatusMessage = "Failed to load technicians. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddTechnicianAsync()
    {
        var dialog = new Views.AddEditTechnicianDialog(null, _technicianService, _logger);
        if (dialog.ShowDialog() == true)
        {
            await LoadAsync();
            StatusMessage = "Technician added successfully.";
        }
    }

    [RelayCommand]
    private async Task EditTechnicianAsync()
    {
        if (SelectedTechnician is null) return;

        var dialog = new Views.AddEditTechnicianDialog(SelectedTechnician, _technicianService, _logger);
        if (dialog.ShowDialog() == true)
        {
            await LoadAsync();
            StatusMessage = "Technician updated successfully.";
        }
    }

    [RelayCommand]
    private async Task DeleteTechnicianAsync()
    {
        if (SelectedTechnician is null) return;

        var techToDelete = SelectedTechnician;
        var confirm = MessageBox.Show(
            $"Are you sure you want to delete '{techToDelete.FullName}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            var (success, error) = await _technicianService.DeleteAsync(techToDelete.Id);
            if (success)
            {
                SelectedTechnician = null;
                await LoadAsync();
                StatusMessage = "Technician deleted.";
            }
            else
            {
                MessageBox.Show(error ?? "Unable to delete technician.", "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting technician Id={Id}", techToDelete.Id);
            StatusMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
        => await LoadAsync();
}
