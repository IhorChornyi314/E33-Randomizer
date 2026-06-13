using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace E33Randomizer;

public partial class MessageDialog : Window
{
    public MessageDialog()
    {
        InitializeComponent();
    }

    private bool _isWindowDragInEffect;
    private Point _cursorPositionAtWindowDragStart = new(0, 0);

    private void WindowDragHandle_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isWindowDragInEffect) return;
        
        Point currentCursorPosition = e.GetPosition(this);
        Point cursorPositionDelta = currentCursorPosition - _cursorPositionAtWindowDragStart;

        Position = this.PointToScreen(cursorPositionDelta);
    }

    private void WindowDragHandle_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control { Name: "WindowDragHandle" or "Header" }) return;
        
        _isWindowDragInEffect = true;
        _cursorPositionAtWindowDragStart = e.GetPosition(this);
    }

    private void WindowDragHandle_OnPointerReleased(object? sender, PointerReleasedEventArgs e) =>
        _isWindowDragInEffect = false;
    
    /// <summary>
    /// Constructs a new DialogViewModel without a cancel button.
    /// </summary>
    /// <param name="owner">The window that the dialog is to be attached to.</param>
    /// <param name="message">The message to show, this should be pre-translated from the Resources.</param>
    /// <param name="title">The title of the dialog box, this should be pre-translated from the Resources.</param>
    /// <param name="buttonConfirmText">The text to use for the confirm button.  This should not be pre-translated.  Just pass the string to be translated (default values can be used from <see cref="MessageBoxButtons" />)</param>
    /// <param name="icon">The icon to show in the box. (use <see cref="MessageBoxIcons"/> for known values, or pass your own) </param>
    public static async Task ShowAsync(Window owner, string message, string title, string buttonConfirmText, string icon)
    {
        var dialog = new MessageDialog();
        var vm = new MessageDialogViewModel(dialog,message,title,buttonConfirmText,icon);
        dialog.DataContext = vm;
        if (!owner.IsVisible)
        {
            dialog.Show();
        }
        else
        {
            await dialog.ShowDialog(owner);
        }
    }
    
    /// <summary>
    /// Constructs a new DialogViewModel without a cancel button that returns a result based on the pressing of the buttons.
    /// </summary>
    /// <param name="owner">The window that the dialog is to be attached to.</param>
    /// <param name="message">The message to show, this should be pre-translated from the Resources.</param>
    /// <param name="title">The title of the dialog box, this should be pre-translated from the Resources.</param>
    /// <param name="buttonConfirmText">The text to use for the confirm button.  This should not be pre-translated.  Just pass the string to be translated (default values can be used from <see cref="MessageBoxButtons" />)</param>
    /// <param name="buttonCancelText">The text to use for the cancel button.  This should not be pre-translated.  Just pass the string to be translated (default values can be used from <see cref="MessageBoxButtons" />)</param>
    /// <remarks>If the owning window is not visible, this will throw an exception as the mechanism used for getting the result can't be used without a visible owner.</remarks>
    /// <param name="icon">The icon to show in the box. (use <see cref="MessageBoxIcons"/> for known values, or pass your own) </param>
    /// <returns>True if the Confirm button is pressed, false if the Cancel Button is pressed.</returns>
    public static async Task<bool> ShowAsyncWithResult(Window owner, string message, string title, string buttonConfirmText, string buttonCancelText, string icon)
    {
        if (!owner.IsVisible)
        {
            throw new Exception(ResourceHelper.GetString(nameof(Assets.Resources.Dialog_WindowNotVisibleException)));
        }
        var dialog = new MessageDialog();
        dialog.DataContext = new MessageDialogViewModel(dialog,message,title,buttonConfirmText,buttonCancelText, icon);
     
        return await dialog.ShowDialog<bool>(owner);
    }

    private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch) return;
        if (DataContext is MessageDialogViewModel vm)
        {
            vm.DesignModeUpdateCancelButton(toggleSwitch.IsChecked ?? false);
        } 
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MessageDialogViewModel vm && e.AddedItems[0] is KeyValuePair<string, string> kvp)
        {
            vm.DesignModeUpdateIcon(kvp.Value);
        }
    }

    private void DesignTimeMessageSelection_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MessageDialogViewModel vm && e.AddedItems[0] is KeyValuePair<string, string> kvp)
        {
            vm.DesignModeUpdateMessage(kvp.Value);
        }
    }

    private async void CopyToClipboardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is MessageDialogViewModel vm && Clipboard is not null)
            {
                var data = new DataTransfer();
                data.Add(DataTransferItem.CreateText(vm.Message));
                await Clipboard.SetDataAsync(data);
            }
        }
        catch (Exception ex)
        {
            await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
        }
    }
}

public static class MessageDialogViewModelDesignData  
{
    public static  MessageDialogViewModel DesignData =>
        new(null!, "This is a test message", "This is the title", MessageBoxButtons.Ok, MessageBoxButtons.Cancel, MessageBoxIcons.Error)
        {
            IsDesignMode = true
        };

    public static List<KeyValuePair<string, string>> DesignMessages =>
    [
        new("Short", "This is a test message"),
        new("Medium", "This is a longer message but still not super long.  Just long enough to hopefully give us some text wrapping."),
        new("Long", "This is a wayyyyy longer message, so long in fact, that I went to get lorem ipsum. Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")
    ];
}