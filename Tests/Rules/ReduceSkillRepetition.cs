using E33Randomizer;

namespace Tests.Rules;

public class ReduceSkillRepetition: OutputRuleBase
{
    private float _threshold = 2.0f;
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.ReduceSkillRepetition) return true;
        
        var randomizedNodes = output.SkillTrees.SelectMany(
            sT => sT.SkillNodes, (_, data) => data).
            Where(sN => config.CustomSkillPlacement.IsRandomized(sN.OriginalSkillCodeName)
            );
        
        var skillsGenerated = randomizedNodes.Select(n => n.SkillData.CodeName);
        var skillFrequencies = TestUtils.CalculateFrequencies(skillsGenerated.ToList());
        var expectedFrequency = 1.0 / skillsGenerated.Distinct().Count();

        var wrongFrequencies = skillFrequencies.Where(kvp => Math.Abs(kvp.Value / expectedFrequency - 1) > _threshold
        );

        if (wrongFrequencies.Any())
        {
            FailureMessage += string.Join(", ", wrongFrequencies.Select(kvp => $"({kvp.Key}:  {kvp.Value})"));
            return false;
        }
        return true;
    }
}