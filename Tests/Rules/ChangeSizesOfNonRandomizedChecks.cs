using E33Randomizer;

namespace Tests.Rules;

public class ChangeSizesOfNonRandomizedChecks: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (config.Settings.ChangeSizesOfNonRandomizedChecks) return true;
        
        foreach (var check in output.Checks)
        {
            var originalCheck = TestLogic.OriginalData.GetCheck(check.Name);
            var checkRandomized =
                originalCheck.Items.Any(i => config.CustomItemPlacement.IsRandomized(i.Item.CodeName)) || originalCheck.Size == 0;
            var checkSize = config.Settings.EnsurePaintedPowerFromPaintress && check.Name.Contains("DA_GA_SQT_RedAndWhiteTree") ? check.Size - 1 : check.Size;
        
            checkSize -= check.Items.Count(iS => Controllers.SkillsController.SkillItems.Contains(iS.Item));
            if (config.Settings.RandomizeStartingWeapons && check.Name == "DT_ChestsContent#Chest_Generic_Chroma") checkSize -= 1;
            if (!checkRandomized && originalCheck.Items.Count != checkSize)
            {
                FailureMessage += $"{originalCheck.Name}'s size shouldn't have changed";
                return false;
            }
        }
        return true;
    }
}