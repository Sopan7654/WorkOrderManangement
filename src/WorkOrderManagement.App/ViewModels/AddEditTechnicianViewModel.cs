using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Windows;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Domain.Entities;

namespace WorkOrderManagement.App.ViewModels;

/// <summary>
/// ViewModel for the Add/Edit Technician dialog.
/// </summary>
public partial class AddEditTechnicianViewModel : BaseViewModel
{
    private readonly ITechnicianService _technicianService;
    private readonly ILogger _logger;
    private readonly bool _isEditMode;
    private readonly int? _existingId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullNameError))]
    private string _fullName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpecialtyError))]
    private string _specialty = string.Empty;

    [ObservableProperty]
    private string _fullNameError = string.Empty;

    [ObservableProperty]
    private string _specialtyError = string.Empty;

    [ObservableProperty]
    private string _generalError = string.Empty;

    public bool DialogResult { get; private set; }
    public string WindowTitle => _isEditMode ? "Edit Technician" : "Add Technician";

    public AddEditTechnicianViewModel(
        Technician? existing,
        ITechnicianService technicianService,
        ILogger logger)
    {
        _technicianService = technicianService;
        _logger = logger;

        if (existing is not null)
        {
            _isEditMode = true;
            _existingId = existing.Id;
            FullName = existing.FullName;
            Specialty = existing.Specialty ?? string.Empty;
        }
    }

    [RelayCommand]
    private async Task SaveAsync(Window window)
    {
        if (!ValidateInput()) return;

        IsBusy = true;
        BusyMessage = _isEditMode ? "Saving changes..." : "Adding technician...";

        try
        {
            var technician = new Technician
            {
                FullName = FullName.Trim(),
                Specialty = string.IsNullOrWhiteSpace(Specialty) ? null : Specialty.Trim()
            };

            if (_isEditMode && _existingId.HasValue)
            {
                technician.Id = _existingId.Value;
                await _technicianService.UpdateAsync(technician);
            }
            else
            {
                await _technicianService.CreateAsync(technician);
            }

            DialogResult = true;
            window.DialogResult = true;
            window.Close();
        }
        catch (Application.ValidationException ex)
        {
            GeneralError = string.Join("\n", ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save technician.");
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

    private bool ValidateInput()
    {
        bool isValid = true;
        FullNameError = string.Empty;
        SpecialtyError = string.Empty;
        GeneralError = string.Empty;

        if (string.IsNullOrWhiteSpace(FullName))
        {
            FullNameError = "Full name is required.";
            isValid = false;
        }
        else if (FullName.Trim().Length > Technician.MaxFullNameLength)
        {
            FullNameError = $"Full name must not exceed {Technician.MaxFullNameLength} characters.";
            isValid = false;
        }

        if (!string.IsNullOrWhiteSpace(Specialty) && Specialty.Length > Technician.MaxSpecialtyLength)
        {
            SpecialtyError = $"Specialty must not exceed {Technician.MaxSpecialtyLength} characters.";
            isValid = false;
        }

        return isValid;
    }
}
