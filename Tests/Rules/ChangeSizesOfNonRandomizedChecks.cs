namespace Tests.Rules;

public class ChangeSizesOfNonRandomizedChecks: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (config.Settings.ChangeSizesOfNonRandomizedChecks) return true;
        
        foreach (var check in output.Checks)
        {
            var originalCheck = TestLogic.OriginalData.GetCheck(check.Name);
            var checkRandomized =
                originalCheck.Items.Any(i => config.CustomItemPlacement.IsRandomized(i.Item.CodeName));
            if (!checkRandomized && originalCheck.Items.Count != check.Items.Count)
            {
                FailureMessage += $"{originalCheck.Name}'s size shouldn't have changed";
                return false;
            }
        }
        return true;
    }
}