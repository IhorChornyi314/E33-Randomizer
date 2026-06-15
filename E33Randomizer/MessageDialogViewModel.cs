using System.Resources;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace E33Randomizer;

public partial class MessageDialogViewModel : ObservableObject
{
    private readonly Window _dialog;
    private static readonly ResourceManager ResourceManager = new (typeof(Assets.Resources)); 

    public string Title { get; }
    
    [ObservableProperty]
    public partial string Message { get; internal set; }
    
    [ObservableProperty]
    public partial Bitmap Icon { get; internal set; }
    public string ButtonConfirmText { get; }
    
    [ObservableProperty]
    public partial string? ButtonCancelText { get; internal set; } = null;

    public bool IsDesignMode { get; set; }

    /// <summary>
    /// Constructs a new DialogViewModel without a cancel button.
    /// </summary>
    /// <param name="dialog">The window that the dialog is attached to.</param>
    /// <param name="message">The message to show, this should be pre-translated from the Resources.</param>
    /// <param name="title">The title of the dialog box, this should be pre-translated from the Resources.</param>
    /// <param name="buttonConfirmText">The text to use for the confirm button.  This should not be pre-translated.  Just pass the string to be translated (default values can be used from <see cref="MessageBoxButtons" />)</param>
    /// <param name="icon">The icon to show in the box. (use <see cref="MessageBoxIcons"/> for known values, or pass your own) </param>
    public MessageDialogViewModel(Window dialog, string message, string title, string buttonConfirmText, string icon)
        : this(dialog, message, title, buttonConfirmText, null, icon)
    {
    }

    /// <summary>
    /// Constructs a new DialogViewModel with a cancel button (if <paramref name="buttonCancelText"/> is not null)
    /// </summary>
    /// <param name="dialog">The window that the dialog is attached to.</param>
    /// <param name="message">The message to show, this should be pre-translated from the Resources.</param>
    /// <param name="title">The title of the dialog box, this should be pre-translated from the Resources.</param>
    /// <param name="buttonConfirmText">The text to use for the confirm button.  This should not be pre-translated.  Just pass the string to be translated (default values can be used from <see cref="MessageBoxButtons" />)</param>
    /// <param name="buttonCancelText">The text to use for the cancel button.  This should not be pre-translated.  Just pass the string to be translated (default values can be used from <see cref="MessageBoxButtons" />)</param>
    /// <param name="icon">The icon to show in the box. (use <see cref="MessageBoxIcons"/> for known values, or pass your own) </param>
    public MessageDialogViewModel(Window dialog, string message, string title, string buttonConfirmText, string? buttonCancelText, string icon)
    {
        _dialog = dialog;
        Title = title;
        Message = message;
        Icon = LoadFromResource(icon);
        ButtonConfirmText = ResourceManager.GetString(buttonConfirmText) ?? "TRANSLATION_MISSING";
        if (buttonCancelText is not null)
        {
            ButtonCancelText = ResourceManager.GetString(buttonCancelText) ?? "TRANSLATION_MISSING";
        }
    }

    [RelayCommand]
    private void Confirm() => _dialog.Close(true);

    [RelayCommand]
    private void Cancel() => _dialog.Close(false);
    
    private static Bitmap LoadFromResource(string resourceUri)
    {
        using var stream = AssetLoader.Open(new Uri(resourceUri));
        return Bitmap.DecodeToWidth(stream, 100);
    }

    /// <summary>
    /// Only works in design mode, used to make it easy to test with and without a cancel button.
    /// </summary>
    /// <param name="input"></param>
    public void DesignModeUpdateCancelButton(bool input)
    {
        if (!IsDesignMode) return;

        ButtonCancelText = input ? ResourceManager.GetString("Button_Cancel") : null;
    }
    
    /// <summary>
    /// Only works in design mode, used to make it easy to test the different options for icons.
    /// </summary>
    /// <param name="icon">Icon to use, see <see cref="MessageBoxIcons"/></param>
    public void DesignModeUpdateIcon(string icon)
    {
        if (!IsDesignMode) return;

        Icon = LoadFromResource(icon);
    }

    /// <summary>
    /// Only works in design mode, used to make it easy to test the different options for text.
    /// </summary>
    /// <param name="message"></param>
    public void DesignModeUpdateMessage(string message)
    {
        if (!IsDesignMode) return;
        Message = message;
    }
}

public static class MessageBoxButtons
{
    public const string Ok = "Button_Ok";
    public const string Cancel = "Button_Cancel";
    public const string Close = "Button_Close";
}

public static class MessageBoxIcons
{
    public static List<KeyValuePair<string, string>> AllIcons =
    [
        new(nameof(Error), Error),
        new(nameof(Information), Information),
    ];

    public const string Error = "avares://E33Randomizer/Assets/Images/DeleteButton.png";
    public const string Information = "avares://E33Randomizer/Assets/Images/JournalIcon.png";
}

