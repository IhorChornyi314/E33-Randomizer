namespace E33Randomizer;

public partial class RandomizeItemsTab : TabBase
{
    public RandomizeItemsTab()
    {
        InitializeComponent();
    }
}

// Design Time Settings DataContext
public static class DesignItemsSettingsViewModel
{
    public static SettingsViewModel SettingsViewModel => new()
    {
        ChangeItemQuantity = true,
        ChangeMerchantInventoryLocked = true,
        ChangeNumberOfChestContents = true,
        ChangeNumberOfActionRewards = true,
        ChangeNumberOfTowerRewards = true,
        ChangeSizeOfNonRandomizedEncounters = true,
        ChangeSizesOfNonRandomizedChecks = true,
        ChangeNumberOfLootDrops = true,
        ChangeMerchantInventorySize = true
    };

}