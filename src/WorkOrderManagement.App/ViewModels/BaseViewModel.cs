using CommunityToolkit.Mvvm.ComponentModel;

namespace WorkOrderManagement.App.ViewModels;

/// <summary>
/// Base class for all ViewModels, providing INotifyPropertyChanged and common helpers.
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    private bool _isBusy;
    private string _busyMessage = "Loading...";

    /// <summary>Indicates whether an async operation is in progress.</summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string BusyMessage
    {
        get => _busyMessage;
        set => SetProperty(ref _busyMessage, value);
    }
}
