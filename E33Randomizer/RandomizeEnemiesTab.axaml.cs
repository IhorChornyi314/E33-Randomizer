namespace E33Randomizer;

public partial class RandomizeEnemiesTab : TabBase
{
    public RandomizeEnemiesTab()
    {
        InitializeComponent();
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