using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;

namespace E33Randomizer.UIControls;

public partial class FormatableStylableTextBlock : TextBlock
{
    
    public static readonly StyledProperty<IEnumerable<string?>?> StringFormatBindingValuesProperty =
        AvaloniaProperty.Register<SubSection, IEnumerable<string?>?>(nameof(StringFormatBindingValues));

    public IEnumerable<string?>? StringFormatBindingValues
    {
        get => GetValue(StringFormatBindingValuesProperty);
        set => SetValue(StringFormatBindingValuesProperty, value);
    }
    
    public static readonly StyledProperty<string?> PreFormattedTextProperty =
        AvaloniaProperty.Register<SubSection, string?>(nameof(PreFormattedText));

    public string? PreFormattedText
    {
        get => GetValue(PreFormattedTextProperty);
        set => SetValue(PreFormattedTextProperty, value);
    }

    static FormatableStylableTextBlock()
    {
        AffectsRender<FormatableStylableTextBlock>(StringFormatBindingValuesProperty);
        AffectsRender<FormatableStylableTextBlock>(PreFormattedTextProperty);
        AffectsRender<FormatableStylableTextBlock>(InlinesProperty);
        AffectsRender<FormatableStylableTextBlock>(TextProperty);

    }

    public FormatableStylableTextBlock()
    {
        PropertyChanged += OnPropertyChanged;
       
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != PreFormattedTextProperty && e.Property != StringFormatBindingValuesProperty) return;
        

        if (string.IsNullOrWhiteSpace(PreFormattedText) || StringFormatBindingValues is null || !StringFormatBindingValues.Any())
            return;
        
        var segments = FormatFindRegex().Split(PreFormattedText);
        if (segments.Length <= 1) return;
        
        Inlines?.Clear();

        Inlines ??= [];

        var tempInlines = new InlineCollection();
        
        int formatOffset = 0;
        foreach (string segment in segments)
        {
            if (FormatFindRegex().IsMatch(segment))
            {
                tempInlines.Add(new Run()
                {
                    Name = $"R{segment[1..^1]}",
                    Text = StringFormatBindingValues?.ElementAt(formatOffset++)
                });
            }
            else
            {
                tempInlines.Add(new Run(segment));
            }
        }
        
        Inlines.AddRange(tempInlines);
    }
    
    [GeneratedRegex(@"(\{\d+\})")]
    private static partial Regex FormatFindRegex();
}