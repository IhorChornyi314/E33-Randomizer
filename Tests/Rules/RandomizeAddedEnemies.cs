using E33Randomizer;

namespace Tests.Rules;

public class RandomizeAddedEnemies: OutputRuleBase
{
    private static int _threshold = 15;
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.EnableEnemyOnslaught) return true;

        var failedEncounters = new List<Encounter> ();
        
        foreach (var encounter in output.RandomizedEncounters)
        {
            var originalEncounterSize = TestLogic.OriginalData.GetEncounter(encounter.Name).Size;
            if (originalEncounterSize >= encounter.Size) continue;
            var addedEnemiesRandomized = false;
            for (int i = originalEncounterSize; i < encounter.Size; i++)
            {
                var sameEnemy = Equals(encounter.Enemies[i], encounter.Enemies[i - originalEncounterSize]);
                if (!sameEnemy)
                {
                    addedEnemiesRandomized = true;
                }
            }
            
            if (addedEnemiesRandomized ^ config.Settings.RandomizeAddedEnemies)
            {
                failedEncounters.Add(encounter);
            }
        }

        if (failedEncounters.Count > _threshold)
        {
            FailureMessage += $"RandomizeAddedEnemies rule was broken {failedEncounters.Count} times, example: {failedEncounters[0]}";
            return false;
        }

        return true;
    }
}