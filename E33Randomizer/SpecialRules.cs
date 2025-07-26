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

    public static Queue<EnemyData> RemainingBossPool = new();

    public static void Reset()
    {
        EnemyData[] bossPoolArray = EnemiesController.GetAllByArchetype("Boss").Concat(EnemiesController.GetAllByArchetype("Alpha")).ToArray();
        RandomizerLogic.rand.Shuffle(bossPoolArray);
        RemainingBossPool = new Queue<EnemyData>(bossPoolArray);
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

    public static void ReplaceCutContentEnemies(Encounter encounter)
    {
        for (int i = 0; i < encounter.Size; i++)
        {
            if (CustomEnemyPlacement.CustomCategoryTranslations["Cut Content Enemies"].Contains(encounter.Enemies[i]))
            {
                var archetype = encounter.Enemies[i].Archetype;
                encounter.Enemies[i] = EnemiesController.GetAllByArchetype(archetype).Find(e => !CustomEnemyPlacement.CustomCategoryTranslations["Cut Content Enemies"].Contains(e));
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

        if (Settings.ReduceBossRepetition && RemainingBossPool.Count > 0)
        {
            for (int i = 0; i < encounter.Size; i++)
            {
                if (encounter.Enemies[i].IsBoss && RemainingBossPool.Count > 0 && CustomEnemyPlacement.NotRandomizedTranslated.Contains(encounter.Enemies[i]))
                {
                    encounter.Enemies[i] = RemainingBossPool.Dequeue();
                }
            }
        }

        if (!Settings.IncludeCutContent)
        {
            ReplaceCutContentEnemies(encounter);
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