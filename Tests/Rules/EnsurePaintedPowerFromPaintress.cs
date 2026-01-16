namespace Tests.Rules;

public class EnsurePaintedPowerFromPaintress: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.EnsurePaintedPowerFromPaintress) return true;

        var check = output.Checks.First(c => c.Name.Contains("DA_GA_SQT_RedAndWhiteTree"));
        if (check.Items.All(i => i.Item.CodeName != "OverPowered"))
        {
            FailureMessage += "DA_GA_SQT_RedAndWhiteTree check did not contain Painted Power";
            return false;
        }
        return true;
    }
}