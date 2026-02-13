namespace Tests.Rules;

public class RandomizeTreeEdges: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (config.Settings.MakeSkillsIntoItems) return true;
        
        var originalSkillEdges = TestLogic.OriginalData.SkillTrees.SelectMany(sT => sT.Edges, (_, data) => data);
        var outputSkillEdges = output.SkillTrees.SelectMany(sT => sT.Edges, (_, data) => data);
        if (!(config.Settings.RandomizeTreeEdges ^
              originalSkillEdges.SequenceEqual(outputSkillEdges)))
        {
            return false;
        }
        // TODO: Include tests for probability and full randomization
        return true;
    }
}