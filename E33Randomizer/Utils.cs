using System.IO;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

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

    public static void WriteAsset(UAsset asset)
    {
        var filePath = asset.FolderName.Value.Replace("/Game/", "randomizer/Sandfall/Content/") + ".uasset";
        string? directoryPath = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        asset.Write(filePath);
    }

    public static int Between(int min, int max)
    {
        return RandomizerLogic.rand.Next(Math.Min(min, max), Math.Max(min, max) + 1);
    }

    public static bool IsBetween(int val, int a, int b)
    {
        return val >= Math.Min(a, b) && val <= Math.Max(a, b);
    }

    public static List<T> ShuffleList<T>(List<T> list)
    {
        var arr = list.ToArray();
        RandomizerLogic.rand.Shuffle(arr);
        return arr.ToList();
    }

    public static void ReplaceNameReference(UAsset asset, string oldName, string newName)
    {
        if (oldName == newName) return;
        var oldNameIndex = asset.SearchNameReference(FString.FromString(oldName));
        asset.SetNameReference(oldNameIndex, FString.FromString(newName));
    }
}