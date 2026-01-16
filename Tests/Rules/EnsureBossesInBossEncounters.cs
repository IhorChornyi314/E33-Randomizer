using E33Randomizer;

namespace Tests.RuleTests;

public class EnsureBossesInBossEncounters: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.EnsureBossesInBossEncounters) return true;

        foreach (var encounter in output.Encounters)
        {
            if (!encounter.IsBossEncounter) continue;
            if (!encounter.Enemies.Any(e => e.IsBoss))
            {
                FailureMessage += $"Encounter {encounter.Name} has no bosses.";
                return false;
            }
        }
        return true;
    }
}