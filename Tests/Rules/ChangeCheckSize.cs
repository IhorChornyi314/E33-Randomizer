using E33Randomizer;
using E33Randomizer.ItemSources;

namespace Tests.Rules;

public class ChangeCheckSize: OutputRuleBase
{
    private Dictionary<string, List<string>> _changedChecksPerCategory = new();
    
    private string GetCheckType(Check check)
    {
        var checkType = "";
        if (check.Name.Contains("DT_Merchant"))
        {
            checkType = "Merchant inventories";
        }
        else if (check.Name.Contains("_GA_"))
        {
            checkType = "Cutscene rewards";
        }
        else if (check.Name.Contains("DT_ChestsContent"))
        {
            checkType = "Map pickups";
        }
        else if (check.Name.Contains("DT_jRPG_Enemies"))
        {
            checkType = "Enemy drops";
        }
        else if (check.Name.Contains("DT_BattleTowerStages"))
        {
            checkType = "Endless tower rewards";
        }
        else if (check.Name.Contains("DT_LootTable_UpgradeItems"))
        {
            checkType = check.Name.Contains("DT_LootTable_UpgradeItems_Exploration") ? "Map pickups" : "Enemy drops";
        }
        else if (check.Name.Contains("DT_LootTable_SkinGustave_Visage"))
        {
            checkType = "Map pickups";
        }
        else if (DialogueItemSource.DialogueRewardPaths.ContainsKey(check.Name.Split('#')[0]))
        {
            checkType = "Dialogue rewards";
        }
        else
        {
            checkType = "Dialogue rewards";
        }

        return checkType;
    }
    
    private bool CheckItems(Check check, Config config)
    {
        var originalSize = TestLogic.OriginalData.GetCheck(check.Name).Size;
        var checkRandomized =
            TestLogic.OriginalData.GetCheck(check.Name).Items.Any(i => config.CustomItemPlacement.IsRandomized(i.Item.CodeName));

        if (!config.Settings.ChangeSizesOfNonRandomizedChecks && !checkRandomized) return true;
        
        var checkType = GetCheckType(check);
        if (!_changedChecksPerCategory.ContainsKey(checkType))
        {
            _changedChecksPerCategory[checkType] = [];
        }

        var checkSize = config.Settings.EnsurePaintedPowerFromPaintress && check.Name.Contains("DA_GA_SQT_RedAndWhiteTree") ? check.Size - 1 : check.Size;
        
        if (originalSize != checkSize)
        {
            _changedChecksPerCategory[checkType].Add(check.Name);
        }
        
        switch (checkType)
        {
            case "Merchant inventories":
                return !config.Settings.ChangeMerchantInventorySize || Utils.IsBetween(checkSize, config.Settings.MerchantInventorySizeMin, config.Settings.MerchantInventorySizeMax);
            case "Cutscene rewards":
                return !config.Settings.ChangeNumberOfActionRewards || Utils.IsBetween(checkSize, config.Settings.ActionRewardsNumberMin, config.Settings.ActionRewardsNumberMax);
            case "Map pickups":
                return !config.Settings.ChangeNumberOfChestContents || Utils.IsBetween(checkSize, config.Settings.ChestContentsNumberMin, config.Settings.ChestContentsNumberMax);
            case "Enemy drops":
                return !config.Settings.ChangeNumberOfLootDrops || Utils.IsBetween(checkSize, config.Settings.LootDropsNumberMin, config.Settings.LootDropsNumberMax);
            case "Endless tower rewards":
                return !config.Settings.ChangeNumberOfTowerRewards || Utils.IsBetween(checkSize, config.Settings.TowerRewardsNumberMin, config.Settings.TowerRewardsNumberMax);
            case "Dialogue rewards":
                return true;
            default:
                FailureMessage += $"{check.Name}'s type could not be determined";
                return false;
        }
    }
    
    public override bool IsSatisfied(Output output, Config config)
    {
        foreach (var check in output.Checks)
        {
            if (!CheckItems(check, config))
            {
                FailureMessage += $"{check.Name}'s size is invalid: {check.Size}";
                return false;
            }
        }

        if (!(config.Settings.ChangeMerchantInventorySize ^ _changedChecksPerCategory["Merchant inventories"].Count == 0))
        {
            FailureMessage += $"{_changedChecksPerCategory["Merchant inventories"].Count} merchant check sizes changed";
            return false;
        }
        if (!(config.Settings.ChangeNumberOfActionRewards ^ _changedChecksPerCategory["Cutscene rewards"].Count == 0))
        {
            FailureMessage += $"{_changedChecksPerCategory["Cutscene rewards"].Count} cutscene check sizes changed";
            return false;
        }
        if (!(config.Settings.ChangeNumberOfChestContents ^ _changedChecksPerCategory["Map pickups"].Count == 0))
        {
            FailureMessage += $"{_changedChecksPerCategory["Map pickups"].Count} chest check sizes changed";
            return false;
        }
        if (!(config.Settings.ChangeNumberOfLootDrops ^ _changedChecksPerCategory["Enemy drops"].Count == 0))
        {
            FailureMessage += $"{_changedChecksPerCategory["Enemy drops"].Count} loot check sizes changed";
            return false;
        }
        if (!(config.Settings.ChangeNumberOfTowerRewards ^ _changedChecksPerCategory["Endless tower rewards"].Count == 0))
        {
            FailureMessage += $"{_changedChecksPerCategory["Endless tower rewards"].Count} tower check sizes changed";
            return false;
        }
        return true;
    }
}