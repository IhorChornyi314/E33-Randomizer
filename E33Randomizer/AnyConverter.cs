using Avalonia.Data.Converters;

namespace E33Randomizer;

public static class Converters
{
    public static IValueConverter HasAny { get; } = 
        new FuncValueConverter<object, bool>(x =>
        {
            if (x is IEnumerable<object?> y)
                return y.Any();
            
            if (x is IEnumerable<object> z)
                return z.Any();
            
            return false;
        });

}