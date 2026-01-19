namespace Tests.Rules;

public class HardcodeSpecialEncounters: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        var danseuseAlphaSummonEncounter = output.GetEncounter("MM_DanseuseAlphaSummon");
        var danseuseCloneEncounter = output.GetEncounter("MM_DanseuseClone*1");
        var danseuseDanceClassCloneEncounter = output.GetEncounter("QUEST_Danseuse_DanceClass_Clone*1");
        
        if (danseuseAlphaSummonEncounter.Size != 2 || danseuseAlphaSummonEncounter.Enemies.Any(e => e.CodeName != "MM_Danseuse_CloneAlpha"))
        {
            FailureMessage += "MM_DanseuseAlphaSummon encounter changed";
            return false;
        }
        if (danseuseCloneEncounter.Size != 1 || danseuseCloneEncounter.Enemies[0].CodeName != "MM_Danseuse_Clone")
        {
            FailureMessage += "MM_DanseuseClone encounter changed";
            return false;
        }
        if (danseuseDanceClassCloneEncounter.Size != 1 || danseuseDanceClassCloneEncounter.Enemies[0].CodeName != "QUEST_Danseuse_DanceClass_Clone")
        {
            FailureMessage += "QUEST_Danseuse_DanceClass_Clone encounter changed";
            return false;
        }
        return true;
    }
}