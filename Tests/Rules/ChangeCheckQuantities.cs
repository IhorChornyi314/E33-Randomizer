using E33Randomizer;
using E33Randomizer.ItemSources;

namespace Tests.Rules;

public class ChangeCheckQuantities: OutputRuleBase
{
    private static List<string> dialoguesWithoutQuantities;
    
    public override bool IsSatisfied(Output output, Config config)
    {
        if (dialoguesWithoutQuantities == null)
        {
            dialoguesWithoutQuantities = TestLogic.OriginalData.Checks.Where(c =>
                DialogueItemSource.DialogueRewardPaths.ContainsKey(c.Name.Split('#')[0]) &&
                !DialogueItemSource.DialogueRewardQuantitiesPaths.ContainsKey(c.Name.Split('#')[0]) && 
                !c.Name.Contains("BP_Dialogue_JudgeOfMercy")
            ).Select(c => c.Name).ToList();
        }
        
        List<string> checksWithItemQuantityChanges = [];
        
        var checks = output.RandomizedChecks.Where(c => !dialoguesWithoutQuantities.Contains(c.Name));
        foreach (var check in checks)
        {
            var originalCheck = TestLogic.OriginalData.GetCheck(check.Name);
            for (int i = 0; i < Math.Min(originalCheck.Size, check.Size); i++)
            {
                if (!check.Items[i].Item.HasQuantities || !originalCheck.Items[i].Item.HasQuantities) continue;

                if (check.Items[i].Quantity != originalCheck.Items[i].Quantity && !checksWithItemQuantityChanges.Contains(check.Name))
                {
                    checksWithItemQuantityChanges.Add(check.Name);
                }

                if (config.Settings.ChangeItemQuantity && check.Items[i].Item.HasQuantities &&
                    !Utils.IsBetween(check.Items[i].Quantity, config.Settings.ItemQuantityMin,
                        config.Settings.ItemQuantityMax))
                {
                    FailureMessage += $"{check.Name} has invalid quantities: {check.Items[i].Quantity}";
                    return false;
                }
            }
        }

        if (!(config.Settings.ChangeItemQuantity ^ checksWithItemQuantityChanges.Count == 0))
        {
            FailureMessage += $"{checksWithItemQuantityChanges.Count} checks have changed item quantities";
            return false;
        }
        
        return true;
    }
}