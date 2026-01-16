namespace Tests.Rules;

public class IncludeCutContentEnemies: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (config.Settings.IncludeCutContentEnemies) return true;
        
        foreach (var encounter in output.Encounters)
        {
            if (encounter.Enemies.Any(e => e.IsCutContent && !encounter.Name.Contains(e.CodeName)))
            {
                FailureMessage += $"{encounter} contains cut content enemies";
                return false;
            }
        }
        return true;
    }
}