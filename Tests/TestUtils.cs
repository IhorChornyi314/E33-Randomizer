namespace Tests;

public static class TestUtils
{
    public static Dictionary<T, float> CalculateFrequencies<T>(List<T> values)
    {
        var counts = new Dictionary<T, int>();
        var totalCount = 0;
        foreach (var value in values)
        {
            counts.TryAdd(value, 0);
            counts[value]++;
            totalCount++;
        }
        var frequencies = counts.Select(kvp => new KeyValuePair<T, float>(kvp.Key, (float)kvp.Value / totalCount)).ToDictionary();
        return frequencies;
    }
    
    public static float CalculateMaxFrequency<T>(List<T> values)
    {
        return CalculateFrequencies(values).Values.Max();
    }
}