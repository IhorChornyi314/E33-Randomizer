namespace Tests.Rules;

public class RandomizeMerchantFights: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (config.Settings.RandomizeMerchantFights) return true;

        foreach (var originalEncounter in TestLogic.OriginalData.Encounters)
        {
            if (!originalEncounter.Enemies.Any(e => e.CodeName.Contains("Merchant_"))) continue;
            if (!originalEncounter.Enemies.SequenceEqual(output.GetEncounter(originalEncounter.Name).Enemies))
            {
                FailureMessage += $"Merchant battle was changed: {originalEncounter}";
                return false;
            }
        }
        return true;
    }
}