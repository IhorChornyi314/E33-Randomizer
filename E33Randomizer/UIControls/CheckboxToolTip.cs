using Avalonia;
using Avalonia.Controls;

namespace E33Randomizer.UIControls;

public class CheckboxToolTip : CheckBox
{
    public static readonly StyledProperty<string?> ToolTipContentProperty =
        AvaloniaProperty.Register<SubSection, string?>(nameof(ToolTipContent));

    public string? ToolTipContent
    {
        get => GetValue(ToolTipContentProperty);
        set => SetValue(ToolTipContentProperty, value);
    }
    
    public static readonly StyledProperty<string?> ToolTipHeaderProperty =
        AvaloniaProperty.Register<SubSection, string?>(nameof(ToolTipHeader));

    public string? ToolTipHeader
    {
        get => GetValue(ToolTipHeaderProperty);
        set => SetValue(ToolTipHeaderProperty, value);
    }

}