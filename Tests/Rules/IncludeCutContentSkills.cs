using E33Randomizer;

namespace Tests.Rules;

public class IncludeCutContentSkills: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (config.Settings.IncludeCutContentSkills) return true;
        
        foreach (var skillTree in output.SkillTrees)
        {
            if (skillTree.Name == "Julie") continue;
            var skillNodes = skillTree.SkillNodes.Where(sN =>
                !Controllers.SkillsController.GetObject(sN.OriginalSkillCodeName).IsCutContent);
            if (skillNodes.Any(sN => sN.SkillData.IsCutContent))
            {
                FailureMessage += $"{skillTree.Name}'s skill tree contains cut content skills";
                return false;
            }
        }
        return true;
    }
}