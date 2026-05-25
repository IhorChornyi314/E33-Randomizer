using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace E33Randomizer.UIControls;

public class SubSection : HeaderedContentControl
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

    
    public static readonly StyledProperty<bool> IsBottomImageVisibleProperty =
        AvaloniaProperty.Register<SubSection, bool>(nameof(IsBottomImageVisible));

    public bool IsBottomImageVisible
    {
        get => GetValue(IsBottomImageVisibleProperty);
        set => SetValue(IsBottomImageVisibleProperty, value);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        IsBottomImageVisible = e.NewSize.Height > 300;
    }
}