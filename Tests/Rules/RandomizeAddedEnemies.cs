namespace Tests.Rules;

public class RandomizeAddedEnemies: OutputRuleBase
{
    private static int _threshold = 10;
    
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.EnableEnemyOnslaught) return true;

        var failedEncounters = 0;
        
        foreach (var encounter in output.Encounters)
        {
            var originalEncounterSize = TestLogic.OriginalData.GetEncounter(encounter.Name).Size;
            for (int i = originalEncounterSize; i < encounter.Size; i++)
            {
                if (i == encounter.Size) break;
                var sameEnemy = Equals(encounter.Enemies[i], encounter.Enemies[i - originalEncounterSize]);
                if (sameEnemy && config.Settings.RandomizeAddedEnemies ||
                    !sameEnemy && !config.Settings.RandomizeAddedEnemies)
                {
                    failedEncounters++;
                }
            }
        }

        if (failedEncounters > _threshold)
        {
            FailureMessage += $"RandomizeAddedEnemies rule was broken {failedEncounters} times.";
            return false;
        }

        return true;
    }
}