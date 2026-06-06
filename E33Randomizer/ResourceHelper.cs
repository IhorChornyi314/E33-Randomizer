using System.Resources;

namespace E33Randomizer;

public static class ResourceHelper
{
    private static readonly ResourceManager ResourceManager = new (typeof(Assets.Resources));

    /// <summary>
    /// Gets the given <paramref name="key" /> from Resources, then runs <see cref="System.String.Format(string, object[])"/> on it using the given <paramref name="args"/> 
    /// </summary>
    /// <param name="key">The key to lookup from Resources</param>
    /// <param name="args">The values to replace placeholders with</param>
    /// <returns>A translated, formatted string</returns>
    public static string GetStringFormatted(string key, params object?[] args)
    {
        var baseString = ResourceManager.GetString(key);
        if (baseString is null)
            return $"TRANSLATION_MISSING for {key}";
        
        return string.Format(baseString, args);
    }

    public static string GetString(string key) => ResourceManager.GetString(key) ?? $"TRANSLATION_MISSING for {key}";
}