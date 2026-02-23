using FluentAssertions;

namespace Tests;

public static class CustomPlacementTestLogic
{
    public static float _frequencyAdjustmentThreshold = 1.75f;
    
    public static bool TestExcluded(Output output, Config config)
    {
        var excludedEnemies = config.CustomEnemyPlacement.ExcludedCodeNames;
        var generatedEnemies = output.RandomizedEncounters.
            SelectMany(e => e.Enemies).Select(e => e.CodeName);
        return !generatedEnemies.Any(e => excludedEnemies.Contains(e));
    }

    public static bool TestNotRandomized(Output output, Config config)
    {
        var notRandomizedEnemies = config.CustomEnemyPlacement.NotRandomizedCodeNames;
        for (int i = 0; i < output.Encounters.Count; i++)
        {
            var originalEncounter = TestLogic.OriginalData.Encounters[i];
            var generatedEncounter = output.Encounters[i];
            for (int j = 0; j < generatedEncounter.Size; j++)
            {
                if (!notRandomizedEnemies.Contains(originalEncounter.Enemies[j % originalEncounter.Size].CodeName)) continue;
                if (originalEncounter.Enemies[j % originalEncounter.Size].CodeName !=
                    generatedEncounter.Enemies[j].CodeName)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool TestFrequencyAdjustment(Output outputNotAdjusted, Output outputAdjusted, Config config)
    {
        var adjustedEnemyPlainNames = config.CustomEnemyPlacement.FrequencyAdjustments.Keys;
        var adjustedEnemyCodeNames = config.CustomEnemyPlacement.PlainNamesToCodeNames(adjustedEnemyPlainNames);

        adjustedEnemyCodeNames = adjustedEnemyCodeNames.Where(cN =>
            !config.CustomEnemyPlacement.NotRandomizedCodeNames.Contains(cN) &&
            !config.CustomEnemyPlacement.ExcludedCodeNames.Contains(cN)).ToList();
        
        var expectedFrequencyChanges =
            config.CustomEnemyPlacement.CustomCategoryDictionaryToCodeNames(config.CustomEnemyPlacement
                .FrequencyAdjustments);
        
        var unadjustedGenerated = outputNotAdjusted.Encounters.SelectMany(e => e.Enemies).Select(e => e.CodeName);
        var adjustedGenerated = outputAdjusted.Encounters.SelectMany(e => e.Enemies).Select(e => e.CodeName);
        
        var unadjustedFrequencies = TestUtils.CalculateFrequencies(unadjustedGenerated.ToList());
        var adjustedFrequencies = TestUtils.CalculateFrequencies(adjustedGenerated.ToList());
        
        unadjustedFrequencies = unadjustedFrequencies.Where(kvp => adjustedEnemyCodeNames.Contains(kvp.Key)).ToDictionary();
        adjustedFrequencies = adjustedFrequencies.Where(kvp => adjustedEnemyCodeNames.Contains(kvp.Key)).ToDictionary();
        
        var frequencyChanges = adjustedFrequencies.Select(kvp => 
            new KeyValuePair<string, float>(kvp.Key, kvp.Value / unadjustedFrequencies[kvp.Key])
        ).ToDictionary();

        var wrongFrequencies = frequencyChanges.Where(kvp =>
            Math.Abs(kvp.Value / expectedFrequencyChanges[kvp.Key] - 1) > _frequencyAdjustmentThreshold
        ).ToDictionary();
        return wrongFrequencies.Count == 0;
    }

    public static bool TestDefaultCustomPlacement(Output output, Config config)
    {
        var originalEnemies = TestLogic.OriginalData.Encounters.SelectMany(e => e.Enemies).Select(e => e.CodeName).ToList();
        var generatedEnemies = output.Encounters.SelectMany(e => e.Enemies).Select(e => e.CodeName).ToList();

        var originalEnemyCategories = originalEnemies.Select(config.CustomEnemyPlacement.GetCategory).ToList();
        var generatedEnemyCategories = generatedEnemies.Select(config.CustomEnemyPlacement.GetCategory).ToList();

        var wrongEnemies = new List<string>();
        
        for (int i = 0; i < originalEnemyCategories.Count; i++)
        {
            if (originalEnemyCategories[i] != generatedEnemyCategories[i])
            {
                wrongEnemies.Add(originalEnemies[i]);
                Console.WriteLine($"Enemy {originalEnemies[i]} has been replaced by {generatedEnemies[i]}");
            }
        }
        
        return originalEnemyCategories.SequenceEqual(generatedEnemyCategories);
    }
}