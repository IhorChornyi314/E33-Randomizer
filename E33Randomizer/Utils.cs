namespace E33Randomizer;

public static class Utils
{
    public static String GetRandomWeighted(Dictionary<String, float> weights, List<String> banned = null)
    {
        banned ??= new List<string>();
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
            if (running >= chance && !banned.Contains(weight.Key))
            {
                return weight.Key;
            }
        }

        return weights.Keys.FirstOrDefault(k => !banned.Contains(k));
    }
    
    public static T Pick<T>(List<T> from)
    {
        return from[RandomizerLogic.rand.Next(from.Count)];
    }

    public static List<String> EnemyDataToCodeNames(List<EnemyData> list)
    {
        var result = new List<String>();
        foreach (var enemyData in list)
        {
            result.Add(enemyData.CodeName);
        }

        return result;
    }
}