using Avalonia.Controls;
using Avalonia.Interactivity;

namespace E33Randomizer;

public partial class RandomizeItemsTab : UserControl
{

    public RandomizeItemsTab()
    {
        InitializeComponent();
    }
    
    private void CustomPlacementButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().OpenCustomPlacementButton_Click(sender, e);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().OpenEditObjectsButton_Click(sender, e);
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