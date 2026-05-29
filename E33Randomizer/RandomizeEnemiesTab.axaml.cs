using Avalonia.Controls;
using Avalonia.Interactivity;

namespace E33Randomizer;

public partial class RandomizeEnemiesTab : UserControl
{
    public RandomizeEnemiesTab()
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
public static class DesignEnemiesSettingsViewModel
{
    public static SettingsViewModel SettingsViewModel => new()
    {
        RandomizeEnemies = true,
        RandomizeEncounterSizes = true,
        EncounterSizeOne = true,
        EncounterSizeTwo = true,
        EncounterSizeThree = true,
        ChangeSizeOfNonRandomizedEncounters = true,
        RandomizeMerchantFights = true,
        IncludeCutContentEnemies = true,
        EnableEnemyOnslaught = true,
        EnemyOnslaughtAdditionalEnemies = 1,
        EnemyOnslaughtEnemyCap = 4,
        EnsureBossesInBossEncounters = true,
        ReduceBossRepetition = true,
        NoSimonP2BeforeLune = true,
        RandomizeAddedEnemies = true
    };
        
}