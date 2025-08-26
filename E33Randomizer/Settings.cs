namespace E33Randomizer;

public static class Settings
{
    public static int Seed = -1;
    public static bool RandomizeEncounterSizes = false;
    public static bool ChangeSizeOfNonRandomizedEncounters = false;
    public static List<int> PossibleEncounterSizes = [1, 2, 3];
    public static string EarliestSimonP2Encounter = "SM_Eveque_ShieldTutorial*1";
    public static bool RandomizeMerchantFights = true;
    public static bool EnableEnemyOnslaught = false;
    public static int EnemyOnslaughtAdditionalEnemies = 1;
    public static int EnemyOnslaughtEnemyCap = 4;

    //public static bool BossNumberCapped = true;
    public static bool RandomizeAddedEnemies = false;
    public static bool EnsureBossesInBossEncounters = false;
    public static bool ReduceBossRepetition = false;
    public static bool TieDropsToEncounters = false; 
    // public static bool EnableJujubreeToSellKeyItems = true;

    public static bool RandomizeItems = true;
}