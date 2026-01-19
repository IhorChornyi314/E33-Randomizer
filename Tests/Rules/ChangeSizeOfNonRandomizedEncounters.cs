namespace Tests.Rules;

public class ChangeSizeOfNonRandomizedEncounters: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (config.Settings is { RandomizeEncounterSizes: false, EnableEnemyOnslaught: false }) return true;
        if (config.Settings.ChangeSizeOfNonRandomizedEncounters) return true;
        
        foreach (var encounter in output.Encounters)
        {
            var originalEncounter = TestLogic.OriginalData.GetEncounter(encounter.Name);
            var encounterRandomized =
                originalEncounter.Enemies.Any(e => config.CustomEnemyPlacement.IsRandomized(e.CodeName));
            if (!encounterRandomized && originalEncounter.Size != encounter.Size)
            {
                FailureMessage += $"{originalEncounter.Name}'s size shouldn't have changed";
                return false;
            }
        }
        return true;
    }
}