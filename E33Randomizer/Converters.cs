using Avalonia.Data.Converters;

namespace E33Randomizer;

public static class Converters
{
    public static readonly IMultiValueConverter MultiBindingToEnumerableConverter =
        new FuncMultiValueConverter<string, IEnumerable<string?>>((inputs) => inputs);

    public static readonly IValueConverter ScrollBarInnerHeightConverter = new FuncValueConverter<double,double>((x) => x-16);
    
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