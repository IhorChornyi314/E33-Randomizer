using System.Globalization;
using System.Resources;
using Avalonia.Data.Converters;

namespace E33Randomizer;

public abstract class ResourceLookupConverter  : IMultiValueConverter
{
    public const string ToolTip = "Tooltip";
    protected abstract string Prefix { get; }
    private static readonly ResourceManager ResourceManager = new (typeof(Assets.Resources)); 
    
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        
        string key = Prefix + string.Join('_', values);

        return ResourceManager.GetString(key);
    }
}