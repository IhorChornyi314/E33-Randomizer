using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace E33Randomizer.UIControls;

public partial class MessageDialogViewModel : ObservableObject
{
    private readonly Window _dialog;

    public string Message { get; }
    public string Icon { get; }
    public string ButtonConfirmText { get; }
    public string ButtonCancelText { get; }

    public MessageDialogViewModel(Window dialog, string message, string title, string buttonConfirmText, string icon)
    {
        _dialog = dialog;
        Message = message;
    }

    [RelayCommand]
    private void Confirm() => _dialog.Close(true);

    [RelayCommand]
    private void Cancel() => _dialog.Close(false);
}

public enum DialogBoxButton {
    OK,
    Cancel
}

public static class MessageBoxIcons
{
    public const string Error = "&#xf030;";
    public const string Information = "&#xf030;";
    public const string Warning = "&#xf030;";
}

