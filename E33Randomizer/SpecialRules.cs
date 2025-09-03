using E33Randomizer.ItemSources;

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
        "SM_FirstLancelier*1",
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

    private static List<string> _prologueDialogues =
    [
        "BP_Dialogue_Eloise", "BP_Dialogue_Gardens_Maelle_FirstDuel", "BP_Dialogue_Harbour_HotelLove",
        "BP_Dialogue_LUAct1_Mime", "BP_Dialogue_Lumiere_ExpFestival_Apprentices", "BP_Dialogue_Lumiere_ExpFestival_Token_Artifact_Colette",
        "BP_Dialogue_Lumiere_ExpFestival_Token_Haircut_Amandine", "BP_Dialogue_Lumiere_ExpFestival_Token_Pictos_Claude", 
        "BP_Dialogue_MainPlaza_Furnitures", "BP_Dialogue_MainPlaza_Trashcan", "BP_Dialogue_Nicolas", "BP_Dialogue_Lumiere_ExpFestival_Apprentices", 
        "BP_Dialogue_Jules",
    ];
    
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
    
    public static void ApplySpecialRulesToEncounter(Encounter encounter)
    {
        if (RandomizerLogic.Settings.NoSimonP2BeforeLune && MandatoryEncounters.Contains(encounter.Name) && MandatoryEncounters.IndexOf(encounter.Name) < 5)
        {
            ApplySimonSpecialRule(encounter);
        }

        if (encounter.Name == "MM_DanseuseAlphaSummon")
        {
            encounter.Enemies = [EnemiesController.GetEnemyData("MM_Danseuse_CloneAlpha"), EnemiesController.GetEnemyData("MM_Danseuse_CloneAlpha")];
        }
        
        if (encounter.Name == "MM_DanseuseClone*1")
        {
            encounter.Enemies = [EnemiesController.GetEnemyData("MM_Danseuse_Clone")];
        }
        
        // if (RandomizerLogic.Settings.BossNumberCapped && !encounter.IsBossEncounter)
        // {
        //     CapNumberOfBosses(encounter);
        // }

        if (RandomizerLogic.Settings.EnsureBossesInBossEncounters && encounter.IsBossEncounter)
        {
            var numberOfBosses = encounter.Enemies.Count(e => e.IsBoss);
            if (numberOfBosses == 0)
            {
                encounter.Enemies[0] = RandomizerLogic.GetRandomByArchetype("Boss");
            }
        }

        if (RandomizerLogic.Settings.ReduceBossRepetition)
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

    public static void ApplySpecialRulesToCheck(CheckData check)
    {
        if (RandomizerLogic.Settings.EnsurePaintedPowerFromPaintress &&
            check.ItemSource.FileName == "DA_GA_SQT_RedAndWhiteTree")
        {
            check.ItemSource.AddItem("BP_GameAction_AddItemToInventory_C_0", ItemsController.GetItemData("OverPowered"));
        }

        if (!RandomizerLogic.Settings.IncludeGearInPrologue && _prologueDialogues.Contains(check.ItemSource.FileName))
        {
            foreach (var itemParticle in check.ItemSource.SourceSections[check.Key])
            {
                if (ItemsController.IsGearItem(itemParticle.Item))
                {
                    itemParticle.Item = ItemsController.GetItemData("UpgradeMaterial_Level1");
                }
            }
        }
    }

    public static bool Randomizable(ItemSource source)
    {
        if (!RandomizerLogic.Settings.RandomizeGestralBeachRewards &&
            (source.FileName.Contains("GestralBeach") || source.FileName.Contains("GestralRace") ||
             source.FileName.Contains("ValleyBall")))
        {
            return false;
        }

        return true;
    }

    public static bool Randomizable(Encounter encounter)
    {
        if (!RandomizerLogic.Settings.RandomizeMerchantFights && encounter.Name.Contains("Merchant"))
        {
            return false;
        }
        return true;
    }
}