using Avalonia.Controls;

namespace E33Randomizer.UIControls;

public partial class MessageDialog : Window
{
    
    public MessageDialog()
    {
        InitializeComponent();
    }

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
    
    public static async Task<T> ShowAsync<T>(Window owner, string message, string title, string buttonConfirmText, string icon)
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
            return await dialog.ShowDialog<T>(owner);
        }

        return default;
    }
    
    [Obsolete("Use ShowAsync(Window) instead")]
    public static void Show(Window owner, string message, string title, string buttonConfirmText, string icon)
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
            dialog.ShowDialog(owner);
        }
    }
}