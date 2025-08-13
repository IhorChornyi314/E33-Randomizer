namespace E33Randomizer;

public static class SpecialRules
{
    public static List<string> NoSimonP2Encounters =
    [
        "SM_Lancelier*1",
        "SM_FirstLancelierNoTuto*1",
        "SM_FirstPortier*1",
        "SM_FirstPortier_NoTuto*1"
    ];

    public static List<string> MandatoryEncounters =
    [
        "SM_Lancelier*1",
        "SM_FirstLancelierNoTuto*1",
        "SM_FirstPortier*1",
        "SM_FirstPortier_NoTuto*1",
        "SM_Eveque_ShieldTutorial*1",
        "SM_Eveque*1",
        "GO_Curator_JumpTutorial*1",
        "GO_Curator_JumpTutorial_NoTuto*1",
        "GO_Goblu",
        "AS_PotatoBag_Boss",
        "QUEST_MatthieuTheColossus*1",
        "QUEST_BertrandBigHands*1",
        "QUEST_DominiqueGiantFeet*1",
        "GV_Sciel*1",
        "EN_Francois",
        "SC_LampMaster",
        "SC_MirrorRenoir_GustaveEnd",
        "FB_Chalier_GradientCounterTutorial*1",
        "FB_Chalier_GradientCounterTutorial_NoTuto*1",
        "FB_DuallisteLR",
        "MS_Monoco",
        "MM_Stalact_GradientAttackTutorial*1",
        "OL_VersoDisappears_Chevaliere*2",
        "OL_MirrorRenoir_FirstFight",
        "MF_Axon_MaskKeeper_VisagesPhase2*1",
        "MF_Axon_Visages",
        "SI_Glissando*1",
        "SI_Axon_Sirene",
        "MM_MirrorRenoir",
        "ML_PaintressIntro",
        "L_Boss_Paintress_P1",
        "L_Boss_Curator_P1"
    ];

    public static List<string> DuelEncounters = [];

    public static List<EnemyData> RemainingBossPool = new();
    private static bool _bossPoolEmpty;

    public static void Reset()
    {
        ResetBossPool();
    }

    private static void ResetBossPool()
    {
        var bossPoolCodeNames = RandomizerLogic.CustomEnemyPlacement.PlainNameToCodeNames["All Bosses"];
        EnemyData[] bossPoolArray = EnemiesController.GetEnemyDataList(bossPoolCodeNames).ToArray();
        RandomizerLogic.rand.Shuffle(bossPoolArray);
        RemainingBossPool = new List<EnemyData>(bossPoolArray);
        var translatedExcluded = EnemiesController.GetEnemyDataList(RandomizerLogic.CustomEnemyPlacement.ExcludedCodeNames);
        RemainingBossPool = RemainingBossPool.Where(e => !translatedExcluded.Contains(e)).ToList();
        if (RemainingBossPool.Count == 0)
        {
            _bossPoolEmpty = true;
        }
    }

    private static EnemyData GetBossReplacement()
    {
        if (RemainingBossPool.Count == 0)
        {
            ResetBossPool();
        }

        if (_bossPoolEmpty)
        {
            return null;
        }

        var result = RemainingBossPool.First();
        RemainingBossPool.Remove(result);
        return result;
    }
    
    public static void ApplySimonSpecialRule(Encounter encounter)
    {
        for (int i = 0; i < encounter.Size; i++)
        {
            if (encounter.Enemies[i].CodeName == "Boss_Simon_Phase2")
            {
                encounter.Enemies[i] = EnemiesController.GetEnemyData("Boss_Simon");
            }
        }
    }

    public static void CapNumberOfBosses(Encounter encounter)
    {
        var numberOfBosses = encounter.Enemies.Count(e => e.IsBoss);
        if (numberOfBosses <= 1)
        {
            return;
        }
        
        for (int i = 0; i < encounter.Size; i++)
        {
            if (encounter.Enemies[i].IsBoss)
            {
                var newEnemy = RandomizerLogic.GetRandomByArchetype("Strong");
                encounter.Enemies[i] = newEnemy;
                numberOfBosses -= 1;
                if (numberOfBosses == 1)
                {
                    return;
                }
            }
        }
    }
    
    public static void ApplySpecialRules(Encounter encounter)
    {
        if (MandatoryEncounters.Contains(encounter.Name) && MandatoryEncounters.IndexOf(encounter.Name) <
            MandatoryEncounters.IndexOf(Settings.EarliestSimonP2Encounter))
        {
            ApplySimonSpecialRule(encounter);
        }

        if (encounter.Name == "MM_DanseuseAlphaSummon")
        {
            encounter.Enemies = [EnemiesController.GetEnemyData("MM_Danseuse_CloneAlpha"), EnemiesController.GetEnemyData("MM_Danseuse_CloneAlpha")];
        }

        // if (Settings.BossNumberCapped && !encounter.IsBossEncounter)
        // {
        //     CapNumberOfBosses(encounter);
        // }

        if (Settings.EnsureBossesInBossEncounters && encounter.IsBossEncounter)
        {
            var numberOfBosses = encounter.Enemies.Count(e => e.IsBoss);
            if (numberOfBosses == 0)
            {
                encounter.Enemies[0] = RandomizerLogic.GetRandomByArchetype("Boss");
            }
        }

        if (Settings.ReduceBossRepetition)
        {
            for (int i = 0; i < encounter.Size; i++)
            {
                if (encounter.Enemies[i].IsBoss && !RandomizerLogic.CustomEnemyPlacement.NotRandomizedCodeNames.Contains(encounter.Enemies[i].CodeName))
                {
                    var newBoss = GetBossReplacement();
                    if (newBoss != null)
                    {
                        encounter.Enemies[i] = newBoss;
                    }
                }
            }
        }
    }

    public static bool Randomizable(Encounter encounter)
    {
        if (!Settings.RandomizeMerchantFights && encounter.Name.Contains("Merchant"))
        {
            return false;
        }
        return true;
    }
}