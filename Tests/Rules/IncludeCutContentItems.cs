namespace Tests.Rules;

public class IncludeCutContentItems: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (config.Settings.IncludeCutContentItems) return true;
        
        foreach (var check in output.Checks)
        {
            if (check.Items.Any(i => i.Item.IsCutContent))
            {
                FailureMessage += $"{check.Name} contains cut content items";
                return false;
            }
        }
        return true;
    }
}