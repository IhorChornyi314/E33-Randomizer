using E33Randomizer;

namespace Tests.RuleTests;

public class ReduceBossRepetition: OutputRuleBase
{
    private float _threshold = 2.0f;
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.ReduceBossRepetition) return true;

        var randomizedEncounterNames =
            TestLogic.OriginalData.Encounters.Where(e => e.Enemies.Any(i => config.CustomEnemyPlacement.IsRandomized(i.CodeName))
            ).Select(e => e.Name);

        var randomizedEncounters = output.Encounters.Where(e => randomizedEncounterNames.Contains(e.Name));
        
        var bossesGenerated = randomizedEncounters.SelectMany(e => e.Enemies, (_, data) => data)
            .Where(e => e.IsBoss).Select(e => e.CodeName);
        var bossFrequencies = TestUtils.CalculateFrequencies(bossesGenerated.ToList());
        var expectedFrequency = 1.0 / bossesGenerated.Distinct().Count();

        var wrongFrequencies = bossFrequencies.Where(kvp => Math.Abs(kvp.Value / expectedFrequency - 1) > _threshold
        );

        if (wrongFrequencies.Any())
        {
            FailureMessage += string.Join(", ", wrongFrequencies.Select(kvp => $"({kvp.Key}:  {kvp.Value})"));
            return false;
        }
        return true;
    }
}