namespace E33Randomizer;

public static class Utils
{
    public static string GetRandomWeighted(Dictionary<string, float> weights, List<string> banned = null)
    {
        banned ??= [];
        float total = 0;
        foreach (var weight in weights)
        {
            total += weight.Value;
        }

        var chance = RandomizerLogic.rand.NextSingle() * total;
        float running = 0;
        foreach (var weight in weights)
        {
            running += weight.Value;
            if (running >= chance && !banned.Contains(weight.Key) && weight.Value > 0.0001)
            {
                return weight.Key;
            }
        }
        return weights.Keys.LastOrDefault(k => !banned.Contains(k));
    }
    
    public static T Pick<T>(List<T> from)
    {
        return from[RandomizerLogic.rand.Next(from.Count)];
    }
}