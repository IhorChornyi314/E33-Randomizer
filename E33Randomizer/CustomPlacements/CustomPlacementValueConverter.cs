using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace E33Randomizer.CustomPlacements;

public class CustomPlacementValueConverter : IMultiValueConverter
{
    
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        string? inputString = null;
        CustomPlacementWindowViewModel? dataContext = null;

        bool? isFound = null;
        
        foreach (var value in values)
        {
            if (value is Tuple<string, bool> tuple)
            {
                inputString = tuple.Item1;
                isFound = tuple.Item2;
            } 

            if (value is CustomPlacementWindowViewModel vm)
                dataContext = vm;
        }

        if (inputString is null && dataContext is not null)
        {
            return new Tuple<string, bool>(dataContext.SelectedCustomPlacementRuleName, isFound! ?? false);
        }
        
        if (inputString is null || dataContext is null)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (!dataContext.CustomPlacementRules.TryGetValue(inputString, out var rule))
        {
            dataContext.CustomPlacementRules.Add(inputString, []);
        }


        if (dataContext.CustomPlacementRules.TryGetValue(inputString, out rule))
        {
            dataContext.SelectedCustomPlacementRule = rule;
            dataContext.SelectedCustomPlacementRuleName = inputString;
            return new Tuple<string, bool>(inputString, isFound!.Value);
        }

        return new BindingNotification(new InvalidCastException("Attempted to create empty rule but failed"));
    }
}