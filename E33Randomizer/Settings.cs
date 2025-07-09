namespace E33Randomizer;

public static class Settings
{
    public static int Seed = -1;
    public static bool RandomizeEnemyTypes = true;
    public static bool RandomizeEncounterSizes = false;
    public static List<int> PossibleEncounterSizes = new List<int> { 1, 2, 3 };
    public static String EarliestSimonP2Encounter = "SM_Eveque_ShieldTutorial*1";
    public static bool RandomizeMerchantFights = true;
    public static bool IncludeCutContent = true;

    public static bool BossNumberCapped = true;
    public static bool EnsureBossesInBossEncounters = true;
    public static bool ReduceBossRepetition = true;
}