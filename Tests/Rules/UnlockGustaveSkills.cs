using NUnit.Framework.Internal;

namespace Tests.Rules;

public class UnlockGustaveSkills: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        var gustaveSkillTree = output.SkillTrees.First(sT => sT.Name == "Gustave");
        
        var originalGustaveNodes = TestLogic.OriginalData.SkillTrees.First(sT => sT.Name == "Gustave").SkillNodes;
        
        foreach (var node in gustaveSkillTree.SkillNodes)
        {
            if (config.Settings.UnlockGustaveSkills && node.IsSecret)
            {
                FailureMessage += $"{node.OriginalSkillCodeName} is locked";
                return false;
            }

            if (!config.Settings.UnlockGustaveSkills && 
                node.IsSecret != originalGustaveNodes.First(n => n.OriginalSkillCodeName == node.OriginalSkillCodeName).IsSecret)
            {
                FailureMessage += $"{node.OriginalSkillCodeName}'s locked status changed";
                return false;
            }
        }
        return true;
    }
}