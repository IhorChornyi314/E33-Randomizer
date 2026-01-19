using E33Randomizer;

namespace Tests.Rules;

public class ReduceGearRepetition: OutputRuleBase
{
    private float _threshold = 2.0f;
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.ReduceGearRepetition) return true;

        
        var gearItemsGenerated = output.RandomizedChecks.SelectMany(c => c.Items, (_, data) => data)
            .Where(i => Controllers.ItemsController.IsGearItem(i.Item)).Select(i => i.Item.CodeName);
        var gearItemFrequencies = TestUtils.CalculateFrequencies(gearItemsGenerated.ToList());
        var expectedFrequency = 1.0 / gearItemsGenerated.Distinct().Count();

        var wrongFrequencies = gearItemFrequencies.Where(kvp => Math.Abs(kvp.Value / expectedFrequency - 1) > _threshold
        );

        if (wrongFrequencies.Any())
        {
            FailureMessage += string.Join(", ", wrongFrequencies.Select(kvp => $"({kvp.Key}:  {kvp.Value})"));
            return false;
        }
        return true;
    }
}