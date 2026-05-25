using Avalonia;
using Avalonia.Controls;

namespace E33Randomizer.UIControls;

public class HeaderToolTip : ToolTip
{
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<SubSection, string?>(nameof(Header));

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}