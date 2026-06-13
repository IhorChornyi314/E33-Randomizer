using System.Globalization;
using Avalonia.Data.Converters;
using AvaloniaEdit.Document;

namespace E33Randomizer.UIControls;

public class AvaloniaEditDocumentStringConverter : IValueConverter
{
    private static readonly Dictionary<string, TextDocument> Documents = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return null;
        
        if (parameter is not string s) throw new ArgumentNullException(nameof(parameter));

        if (Documents.TryGetValue(s, out var document))
        {
            document.Text = value as string;
        }
        else
        {
            document = new TextDocument(value as string);
            Documents.Add(s, document);
        }
        
        return document;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}