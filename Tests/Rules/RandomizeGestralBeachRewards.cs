using ZstdSharp.Unsafe;

namespace Tests.Rules;

public class RandomizeGestralBeachRewards: OutputRuleBase
{
    private static List<string> _gestralBeachCheckNames =
    [
        "BP_Dialogue_GestralBeach_Climb_GrandisMain#SkinMaelle_Bikini",
        "BP_Dialogue_GestralBeach_OnlyUp_Top#SkinMonoco_Bikini",
        "BP_Dialogue_GestralBeach_WipeoutGestral2_End#SkinLune_BikiniA",
        "BP_Dialogue_GestralBeach_WipeoutGestral2_End#SkinGustave_Bikini",
        "BP_Dialogue_GestralRace#SkinVerso_BikiniB",
        "BP_Dialogue_GestralRace#UpgradeMaterial_Level3",
        "BP_Dialogue_GestralRace#UpgradeMaterial_Level4",
    ];
    
    public override bool IsSatisfied(Output output, Config config)
    {
        var failedChecks = 0;
        foreach (var checkName in _gestralBeachCheckNames)
        {
            var originalCheck = TestLogic.OriginalData.GetCheck(checkName);
            var outputCheck = output.GetCheck(checkName);

            var originalItems = originalCheck.Items.Select(i => i.Item.CodeName);
            var outputItems = outputCheck.Items.Select(i => i.Item.CodeName);

            if (!(config.Settings.RandomizeGestralBeachRewards ^ Enumerable.SequenceEqual(originalItems, outputItems)))
            {
                failedChecks++;
            }
        }

        if (failedChecks > _gestralBeachCheckNames.Count / 2)
        {
            FailureMessage += $"{failedChecks} checks failed";
            return false;
        }
        
        return true;
    }
}