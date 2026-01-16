using E33Randomizer;

namespace Tests.RuleTests;

public class ChangeEncounterSize: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        var settings = config.Settings;
        if (!settings.RandomizeEncounterSizes)
        {
            return true;
        }
        List<int> possibleEncounterSizes = new List<int>();
        var onslaughtEnemies = settings.EnableEnemyOnslaught ? settings.EnemyOnslaughtAdditionalEnemies : 0;
        var maxEncounterSize = settings.EnableEnemyOnslaught ? settings.EnemyOnslaughtEnemyCap : 3;
        if (settings.EncounterSizeOne)
        {
            possibleEncounterSizes.Add(1 + onslaughtEnemies);
        }
        if (settings.EncounterSizeTwo)
        {
            possibleEncounterSizes.Add(2 + onslaughtEnemies);
        }
        if (settings.EncounterSizeThree)
        {
            possibleEncounterSizes.Add(3 + onslaughtEnemies);
        }

        foreach (var encounter in output.Encounters)
        {
            if (!settings.ChangeSizeOfNonRandomizedEncounters)
            {
                var originalEncounter = TestLogic.OriginalData.GetEncounter(encounter.Name);
                var encounterRandomized =
                    originalEncounter.Enemies.Any(e => config.CustomEnemyPlacement.IsRandomized(e.CodeName));
                if (!encounterRandomized)
                {
                    if (originalEncounter.Size != encounter.Size)
                    {
                        FailureMessage += $"{originalEncounter.Name}'s size shouldn't have changed";
                        return false;
                    }
                    continue;
                }
            }
            
            if (!possibleEncounterSizes.Contains(encounter.Size))
            {
                FailureMessage += $"{encounter.Name} encounter size was invalid: {encounter.Size}";
                return false;
            }

            if (encounter.Size > maxEncounterSize)
            {
                FailureMessage += $"{encounter.Name} encounter size was too big: {encounter.Size}";
                return false;
            }
        }

        return true;
    }
}