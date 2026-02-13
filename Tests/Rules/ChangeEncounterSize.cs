using E33Randomizer;

namespace Tests.RuleTests;

public class ChangeEncounterSize: OutputRuleBase
{
    private Dictionary<string, int> _fixedEncounterSizes = new ()
    {
        {"MM_DanseuseAlphaSummon", 2},
        {"MM_DanseuseClone*1", 1},
        {"QUEST_Danseuse_DanceClass_Clone*1", 1},
    };
    
    public override bool IsSatisfied(Output output, Config config)
    {
        var settings = config.Settings;
        List<int> possibleEncounterSizes = new List<int>();
        var onslaughtEnemies = settings.EnableEnemyOnslaught ? settings.EnemyOnslaughtAdditionalEnemies : 0;
        var maxEncounterSize = settings.EnableEnemyOnslaught ? settings.EnemyOnslaughtEnemyCap : 4;
        if (settings.EncounterSizeOne)
        {
            possibleEncounterSizes.Add(Math.Min(1 + onslaughtEnemies, maxEncounterSize));
        }
        if (settings.EncounterSizeTwo)
        {
            possibleEncounterSizes.Add(Math.Min(2 + onslaughtEnemies, maxEncounterSize));
        }
        if (settings.EncounterSizeThree)
        {
            possibleEncounterSizes.Add(Math.Min(3 + onslaughtEnemies, maxEncounterSize));
        }

        foreach (var encounter in output.Encounters)
        {
            if (TestLogic.SpecialCaseEncounterNames.Contains(encounter.Name)) continue;
            if (!settings.RandomizeMerchantFights && encounter.Name.Contains("Merchant")) continue;
            
            var originalEncounter = TestLogic.OriginalData.GetEncounter(encounter.Name);
            if (!settings.RandomizeEncounterSizes)
            {
                possibleEncounterSizes = [Math.Min(originalEncounter.Size + onslaughtEnemies, maxEncounterSize)];
            }
            
            if (!settings.ChangeSizeOfNonRandomizedEncounters)
            {
                var encounterRandomized = output.RandomizedEncounters.Any(e => e.Name == originalEncounter.Name);
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