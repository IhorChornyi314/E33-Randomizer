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

    public static readonly List<string> MandatoryEncounters =
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

    public static readonly List<string> MerchantEncounters =
    [
        "Merchant_GoblusLair",
        "Merchant_GestralVillage1",
        "Merchant_GestralVillage2",
        "Merchant_GestralVillage3",
        "Merchant_SeaCliff",
        "Merchant_ForgottenBattlefield",
        "Merchant_GrandisStation",
        "Merchant_OldLumiere",
        "Merchant_Visages",
        "Merchant_Sirene",
        "Merchant_Monolith",
        "Merchant_Lumiere",
        "Merchant_YellowForest",
        "Merchant_OrangeForest",
        "Merchant_MonocosMountain",
        "Merchant_Reacher",
        "Merchant_CleaIsland",
        "Merchant_Optional1",
        "Merchant_Optional2",
        "Merchant_Optional3",
        "Merchant_TwilightSanctuary",
        "YF_Limonsol",
        "SC_Gestral_Sonnyso*1"
    ];

    public static List<string> DuelEncounters = [];

    public static readonly List<string> CutContentEnemies =
    [
        "AS_Gestral_Dragoon",
        "AS_GestralBully_A",
        "AS_GestralBully_B",
        "AS_GestralBully_C",
        "SC_FearLight",
        "SC_SapNevronBoss",
        "SC_Gestral_Sonnyso",
        "FB_DuallisteR",
        "FB_DuallisteL",
        "Test_PlaceHolderBattleDude",
        "SM_Lancelier_AlternatifA",
        "SM_Lancelier_AlternatifB",
        "SM_Lancelier_AlternatifC",
        "SM_Lancelier_AlternatifD",
        "TestMichel_Lancelier",
        "YF_Gault_AlternativA",
        "YF_Gault_AlternativB",
        "YF_Gault_AlternativC",
        "YF_Gault_AlternativD",
        "YF_Gault_AlternativE",
        "SM_Portier_AlternativA",
        "SM_Portier_AlternativB",
        "SM_Portier_AlternativC",
        "SM_Portier_AlternativD",
        "SM_Portier_AlternativE",
        "SM_Volester_AlternativA",
        "SM_Volester_AlternativB",
        "SM_Volester_AlternativC",
        "SM_Volester_AlternativD",
        "SM_Volester_AlternativE",
        "YF_Potier_AlternativeA",
        "YF_Potier_AlternativeB",
        "YF_Potier_AlternativeC",
        "YF_Potier_AlternativeD",
        "YF_Potier_AlternativeE",
        "YF_Sapling_AlternativeA",
        "YF_Sapling_AlternativeB",
        "YF_Sapling_AlternativeC",
        "YF_Jar_AlternativeA",
        "YF_Jar_AlternativeB",
        "YF_Jar_AlternativeC",
        "CZ_ChromaMaelle",
        "CZ_ChromaVerso",
        "CZ_ChromaLune",
        "CZ_ChromaSciel",
        "CZ_ChromaMonoco"
    ];

    public static Queue<EnemyData> RemainingBossPool = new();

    public static void Reset()
    {
        EnemyData[] bossPoolArray = RandomizerLogic.GetAllByArchetype("Boss").Concat(RandomizerLogic.GetAllByArchetype("Alpha")).ToArray();
        RandomizerLogic.rand.Shuffle(bossPoolArray);
        RemainingBossPool = new Queue<EnemyData>(bossPoolArray);
    }
    
    public static void ApplySimonSpecialRule(Encounter encounter)
    {
        for (int i = 0; i < encounter.Size; i++)
        {
            if (encounter.Enemies[i].CodeName == "Boss_Simon_Phase2")
            {
                encounter.SetEnemy(i, RandomizerLogic.GetEnemyData("Boss_Simon"));
            }
        }
    }

    public static void ReplaceCutContentEnemies(Encounter encounter)
    {
        for (int i = 0; i < encounter.Size; i++)
        {
            if (CutContentEnemies.Contains(encounter.Enemies[i].CodeName))
            {
                var archetype = encounter.Enemies[i].Archetype;
                encounter.SetEnemy(i, RandomizerLogic.GetAllByArchetype(archetype).Find(e => !CutContentEnemies.Contains(e.CodeName)));
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
                encounter.SetEnemy(i, newEnemy);
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

        if (Settings.BossNumberCapped && !encounter.IsBossEncounter)
        {
            CapNumberOfBosses(encounter);
        }

        if (Settings.EnsureBossesInBossEncounters && encounter.IsBossEncounter)
        {
            var numberOfBosses = encounter.Enemies.Count(e => e.IsBoss);
            if (numberOfBosses == 0)
            {
                encounter.SetEnemy(0, RandomizerLogic.GetRandomByArchetype("Boss"));
            }
        }

        if (Settings.ReduceBossRepetition && RemainingBossPool.Count > 0)
        {
            for (int i = 0; i < encounter.Size; i++)
            {
                if (encounter.Enemies[i].IsBoss && RemainingBossPool.Count > 0 && CustomEnemyPlacement.NotRandomizedTranslated.Contains(encounter.Enemies[i]))
                {
                    encounter.SetEnemy(i, RemainingBossPool.Dequeue());
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
        if (!Settings.RandomizeMerchantFights && MerchantEncounters.Contains(encounter.Name))
        {
            return false;
        }
        return true;
    }
}