namespace Tests.Rules;

public class GuaranteeGustaveOvercharge: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.GuaranteeGustaveOvercharge) return true;

        var gustaveTree = output.SkillTrees.Find(sT => sT.Name == "Gustave");
        var overchargeNode =
            gustaveTree.SkillNodes.Find(sN => sN.OriginalSkillCodeName == "DA_Skill_Gustave_UnleashCharge");
        if (overchargeNode.SkillData.CodeName != "DA_Skill_Gustave_UnleashCharge")
        {
            FailureMessage += "Overcharge node does not contain Overcharge";
            return false;
        }

        return true;
    }
}