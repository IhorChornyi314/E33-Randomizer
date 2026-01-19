namespace Tests.Rules;

public class ReduceKeyItemRepetition: OutputRuleBase
{
    private float _threshold = 2.0f;
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.ReduceKeyItemRepetition) return true;

        var keyItemsGenerated = output.RandomizedChecks.SelectMany(c => c.Items, (_, data) => data)
            .Where(i => i.Item.CustomName.Contains("Key Item")).Select(i => i.Item.CodeName);
        var keyItemFrequencies = TestUtils.CalculateFrequencies(keyItemsGenerated.ToList());
        var expectedFrequency = 1.0 / keyItemsGenerated.Distinct().Count();

        var wrongFrequencies = keyItemFrequencies.Where(kvp => Math.Abs(kvp.Value / expectedFrequency - 1) > _threshold
        );

        if (wrongFrequencies.Any())
        {
            FailureMessage += string.Join(", ", wrongFrequencies.Select(kvp => $"({kvp.Key}:  {kvp.Value})"));
            return false;
        }
        return true;
    }
}