namespace Tests.Rules;

public class RandomizeSkillUnlockCosts: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (config.Settings.MakeSkillsIntoItems) return true;
        
        var originalSkillNodes = TestLogic.OriginalData.SkillTrees.SelectMany(sT => sT.SkillNodes, (_, data) => data).Select(s => s.UnlockCost).ToList();
        var outputSkillNodes = output.SkillTrees.SelectMany(sT => sT.SkillNodes, (_, data) => data).Select(s => s.UnlockCost).ToList();
        if (!(config.Settings.RandomizeSkillUnlockCosts ^
              originalSkillNodes.SequenceEqual(outputSkillNodes)))
        {
            return false;
        }
        return true;
    }
}