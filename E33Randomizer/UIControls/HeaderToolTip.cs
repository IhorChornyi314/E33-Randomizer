using Avalonia;
using Avalonia.Controls;
using E33Randomizer.Assets;

namespace E33Randomizer.UIControls;

public class HeaderToolTip : ToolTip
{
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<SubSection, string?>(nameof(Header));

    /// <summary>
    /// Defaults to <see cref="Resources.ToolTip_Header_HowItWorks"/>
    /// </summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}