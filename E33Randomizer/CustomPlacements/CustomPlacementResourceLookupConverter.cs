using System.Globalization;
using System.Resources;
using Avalonia.Data.Converters;

namespace E33Randomizer.CustomPlacements;

public class CustomPlacementResourceLookupConverter  : IMultiValueConverter
{
    private static readonly ResourceManager ResourceManager = new (typeof(Assets.Resources)); 
    
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        
        string key = "CustomPlacement_" + string.Join('_', values);

        return ResourceManager.GetString(key);
    }
}