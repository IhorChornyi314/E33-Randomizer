using System.Configuration;
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
    
    public static Dictionary<string, string> BrokenEnemyReplacements = new Dictionary<string, string>()
    {
        {"QUEST_WeaponlessChalier", "FC_ChalierSword"},
        {"FB_Dualliste_Phase1", "FB_DuallisteLR"},
        {"YF_Jar_AlternativeA", "YF_Jar_AlternativeC"},
        {"YF_Jar_AlternativeB", "YF_Jar_AlternativeC"},
        {"SM_Volester_AlternativA", "SM_Volester_AlternativD"},
        {"SM_Volester_AlternativB", "SM_Volester_AlternativD"},
        {"SM_Volester_AlternativC", "SM_Volester_AlternativD"},
        {"Petank_Parent", "Petank_Blue"},
    };
    
    public static Dictionary<string, string> BrokenItemReplacements = new Dictionary<string, string>()
    {
        {"MitigatedPerfection", "Verleso"},
        {"Chroma", "ChromaPack_Large"},
        {"04_Key_Placeholder", "Quest_OldKey"},
        {"Gold_Small", "ChromaPack_Regular"},
        {"Gold_Medium", "ChromaPack_Large"},
        {"Gold_Big", "ChromaPack_ExtraLarge"},
        {"Consumable_SkillPoint", "Consumable_LuminaPoint"},
    };

    public static List<string> DuelEncounters = [];

    private static ObjectPool<EnemyData> _bossPool;
    private static ObjectPool<ItemData> _keyItemsPool, _gearItemsPool;
    private static List<SkillData> cutContentSkills = new();
    private static List<ItemData> cutContentItems = new();
    private static List<EnemyData> cutContentEnemies = new();
    private static Dictionary<string, ObjectPool<SkillData>> _skillCategoryPools = new();

    
    private static List<string> _prologueDialogues =
    [
        "BP_Dialogue_Eloise", "BP_Dialogue_Gardens_Maelle_FirstDuel", "BP_Dialogue_Harbour_HotelLove",
        "BP_Dialogue_LUAct1_Mime", "BP_Dialogue_Lumiere_ExpFestival_Apprentices", "BP_Dialogue_Lumiere_ExpFestival_Token_Artifact_Colette",
        "BP_Dialogue_Lumiere_ExpFestival_Token_Haircut_Amandine", "BP_Dialogue_Lumiere_ExpFestival_Token_Pictos_Claude", 
        "BP_Dialogue_MainPlaza_Furnitures", "BP_Dialogue_MainPlaza_Trashcan", "BP_Dialogue_Nicolas", "BP_Dialogue_Lumiere_ExpFestival_Apprentices", 
        "BP_Dialogue_Jules", "BP_Dialogue_Lumiere_ExpFestival_Maelle", "BP_Dialogue_Richard"
    ];
    
    public static void Reset()
    {
        cutContentSkills = Controllers.SkillsController.ObjectsData.Where(s => s.IsCutContent).ToList();
        cutContentEnemies = Controllers.EnemiesController.ObjectsData.Where(s => s.IsCutContent).ToList();
        cutContentItems = Controllers.ItemsController.ObjectsData.Where(s => s.IsCutContent).ToList();
        
        var bannedBosses = Controllers.EnemiesController.GetObjects(RandomizerLogic.CustomEnemyPlacement.ExcludedCodeNames);
        bannedBosses.AddRange(Controllers.EnemiesController.GetObjects(RandomizerLogic.BrokenEnemies));
        if (!RandomizerLogic.Settings.IncludeCutContentEnemies)
        {
            bannedBosses.AddRange(cutContentEnemies);
        }

        _bossPool = new ObjectPool<EnemyData>(
            Controllers.EnemiesController.GetObjects(RandomizerLogic.CustomEnemyPlacement.PlainNameToCodeNames["All Bosses"]), bannedBosses);

        var bannedItems = Controllers.ItemsController.GetObjects(RandomizerLogic.CustomItemPlacement.ExcludedCodeNames);
        bannedItems.AddRange(Controllers.ItemsController.GetObjects(RandomizerLogic.BrokenItems));
        if (!RandomizerLogic.Settings.IncludeCutContentItems)
        {
            bannedItems.AddRange(cutContentItems);
        }
        _keyItemsPool = new ObjectPool<ItemData>(
            Controllers.ItemsController.GetObjects(RandomizerLogic.CustomItemPlacement.PlainNameToCodeNames["Key Item"]),
            bannedItems
            );
        var gearItems = new List<ItemData>(Controllers.ItemsController.GetObjects(RandomizerLogic.CustomItemPlacement.PlainNameToCodeNames["Weapon"]));
        gearItems.AddRange(Controllers.ItemsController.GetObjects(RandomizerLogic.CustomItemPlacement.PlainNameToCodeNames["Pictos"]));
        
        _gearItemsPool = new ObjectPool<ItemData>(gearItems, bannedItems);
    }

    public static void ResetSkillsPool()
    {
        _skillCategoryPools = new Dictionary<string, ObjectPool<SkillData>>();
    }
    
    public static void ApplySimonSpecialRule(Encounter encounter)
    {
        for (int i = 0; i < encounter.Size; i++)
        {
            if (encounter.Enemies[i].CodeName == "Boss_Simon_Phase2" || encounter.Enemies[i].CodeName == "Boss_Simon_ALPHA")
            {
                encounter.Enemies[i] = Controllers.EnemiesController.GetObject("Boss_Simon");
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

    public static void ReplaceBrokenEnemies(Encounter encounter)
    {
        for (int i = 0; i < encounter.Size; i++)
        {
            if (BrokenEnemyReplacements.TryGetValue(encounter.Enemies[i].CodeName, out string? value))
            {
                encounter.Enemies[i] = Controllers.EnemiesController.GetObject(value);
            }
        }
    }
    
    public static void ApplySpecialRulesToEncounter(Encounter encounter)
    {
        if (RandomizerLogic.Settings.EnsureBossesInBossEncounters && encounter.IsBossEncounter)
        {
            var numberOfBosses = encounter.Enemies.Count(e => e.IsBoss);
            if (numberOfBosses == 0)
            {
                //This will ignore custom placement 
                encounter.Enemies[0] = Utils.Pick(Controllers.EnemiesController.GetAllByArchetype("Boss")); // RandomizerLogic.GetRandomByArchetype("Boss");
            }
        }
        
        
        // if (RandomizerLogic.Settings.BossNumberCapped && !encounter.IsBossEncounter)
        // {
        //     CapNumberOfBosses(encounter);
        // }

        if (RandomizerLogic.Settings.ReduceBossRepetition)
        {
            for (int i = 0; i < encounter.Size; i++)
            {
                if (encounter.Enemies[i].IsBoss && !RandomizerLogic.CustomEnemyPlacement.NotRandomizedCodeNames.Contains(encounter.Enemies[i].CodeName))
                {
                    var newBoss = _bossPool.GetObject();
                    if (newBoss != null)
                    {
                        encounter.Enemies[i] = newBoss;
                    }
                }
            }
        }
        
        if (RandomizerLogic.Settings.NoSimonP2BeforeLune && MandatoryEncounters.Contains(encounter.Name) && MandatoryEncounters.IndexOf(encounter.Name) < 5)
        {
            ApplySimonSpecialRule(encounter);
        }
        
        ReplaceBrokenEnemies(encounter);
        
        if (encounter.Name != "Boss_Duolliste_P2")
        {
            encounter.Enemies = encounter.Enemies.Select(e =>
                e.CodeName == "Duolliste_P2" ? Controllers.EnemiesController.GetObject("Duolliste_A") : e).ToList();
        }

        switch (encounter.Name)
        {
            case "MM_DanseuseAlphaSummon":
                encounter.Enemies = [Controllers.EnemiesController.GetObject("MM_Danseuse_CloneAlpha"), Controllers.EnemiesController.GetObject("MM_Danseuse_CloneAlpha")];
                break;
            case "MM_DanseuseClone*1":
                encounter.Enemies = [Controllers.EnemiesController.GetObject("MM_Danseuse_Clone")];
                break;
            case "QUEST_Danseuse_DanceClass_Clone*1":
                encounter.Enemies = [Controllers.EnemiesController.GetObject("QUEST_Danseuse_DanceClass_Clone")];
                break;
        }
    }
    
    public static void ReplaceBrokenItems(CheckData check)
    {
        foreach (var itemParticle in check.ItemSource.SourceSections[check.Key])
        {
            if (!RandomizerLogic.BrokenItems.Contains(itemParticle.Item.CodeName)) continue;
            
            if (BrokenItemReplacements.TryGetValue(itemParticle.Item.CodeName, out string? value))
            {
                itemParticle.Item = Controllers.ItemsController.GetObject(value);
            }
            else if (itemParticle.Item.CodeName.Contains("Foot"))
            {
                itemParticle.Item = Controllers.ItemsController.GetObject("StalactFoot");
            }
            else
            {
                var newItem = _gearItemsPool.GetObject();
                if (newItem != null)
                    itemParticle.Item = newItem;
            }
        }
    }

    public static void ApplySpecialRulesToCheck(CheckData check)
    {
        if (!RandomizerLogic.Settings.IncludeGearInPrologue && 
            (_prologueDialogues.Contains(check.ItemSource.FileName) || 
             check.Key.Contains("Chest_Lumiere_ACT1") ||
             check.ItemSource.FileName == "DA_GA_SQT_TheGommage"
             ))
        {
            foreach (var itemParticle in check.ItemSource.SourceSections[check.Key])
            {
                if (Controllers.ItemsController.IsGearItem(itemParticle.Item))
                {
                    itemParticle.Item = Controllers.ItemsController.GetObject("UpgradeMaterial_Level1");
                }
            }
        }

        if (RandomizerLogic.Settings.ReduceKeyItemRepetition)
        {
            foreach (var itemParticle in check.ItemSource.SourceSections[check.Key])
            {
                if (itemParticle.Item.CustomName.Contains("Key Item"))
                {
                    var newItem = _keyItemsPool.GetObject();
                    if (newItem != null)
                        itemParticle.Item = newItem;
                }
            }
        }

        if (RandomizerLogic.Settings.ReduceGearRepetition)
        {
            foreach (var itemParticle in check.ItemSource.SourceSections[check.Key])
            {
                if (Controllers.ItemsController.IsGearItem(itemParticle.Item))
                {
                    var newItem = _gearItemsPool.GetObject();
                    if (newItem != null)
                        itemParticle.Item = newItem;
                }
            }
        }
        
        ReplaceBrokenItems(check);
        
        if (RandomizerLogic.Settings.EnsurePaintedPowerFromPaintress &&
            check.ItemSource.FileName == "DA_GA_SQT_RedAndWhiteTree")
        {
            check.ItemSource.AddItem("BP_GameAction_AddItemToInventory_C_0", Controllers.ItemsController.GetObject("OverPowered"));
        }
    }

    private static SkillData GetReplacedSkillPool(SkillData replacedSkill, string skillCategory)
    {
        var replacedSkillName = replacedSkill.CodeName;
        var replacedSkillCategory = replacedSkillName;
            
        foreach (var categoryName in RandomizerLogic.CustomSkillPlacement.CustomPlacementRules[skillCategory].Keys)
        {
            if (RandomizerLogic.CustomSkillPlacement.PlainNameToCodeNames[categoryName]
                .Contains(replacedSkillName))
            {
                replacedSkillCategory = categoryName;
                break;
            }
        }
            
        if (!_skillCategoryPools.ContainsKey(replacedSkillCategory))
        {
            var possibleReplacementCodeNames = RandomizerLogic.CustomSkillPlacement.PlainNameToCodeNames.GetValueOrDefault(replacedSkillCategory, [replacedSkillName]);
            var possibleReplacements = Controllers.SkillsController.GetObjects(possibleReplacementCodeNames);
            
            var excludedPool = Controllers.SkillsController.GetObjects(RandomizerLogic.CustomSkillPlacement.ExcludedCodeNames);

            if (!RandomizerLogic.Settings.IncludeCutContentSkills)
            {
                excludedPool.AddRange(cutContentSkills);
            }
            
            _skillCategoryPools[replacedSkillCategory] = new ObjectPool<SkillData>(possibleReplacements, excludedPool);
        }

        return _skillCategoryPools[replacedSkillCategory].GetObject();
    }
    public static void ApplySpecialRulesToSkillNode(SkillNode node)
    {
        if (RandomizerLogic.Settings.ReduceSkillRepetition && !RandomizerLogic.CustomSkillPlacement.NotRandomizedCodeNames.Contains(node.OriginalSkillCodeName))
        {
            var skillCategory = RandomizerLogic.CustomSkillPlacement.GetCategory(node.OriginalSkillCodeName);

            if (!RandomizerLogic.CustomSkillPlacement.CustomPlacementRules.ContainsKey(skillCategory))
            {
                if (!_skillCategoryPools.ContainsKey("Default"))
                {
                    var defaultReplacements = RandomizerLogic.CustomSkillPlacement.DefaultFrequencies.Where(x => x.Value > 0.0001).Select(x => x.Key);
                    
                    var excludedPool = Controllers.SkillsController.GetObjects(RandomizerLogic.CustomSkillPlacement.ExcludedCodeNames);

                    if (!RandomizerLogic.Settings.IncludeCutContentSkills)
                    {
                        excludedPool.AddRange(cutContentSkills);
                    }
                    
                    _skillCategoryPools["Default"] = new ObjectPool<SkillData>(Controllers.SkillsController.GetObjects(defaultReplacements), excludedPool);
                }

                skillCategory = "Default";
            }
            
            var newSkill = skillCategory != "Default" ? GetReplacedSkillPool(node.SkillData, skillCategory) : _skillCategoryPools["Default"].GetObject();
            
            if (newSkill != null)
            {
                node.SkillData = newSkill;
            }
        }

        if (RandomizerLogic.Settings.UnlockGustaveSkills && node.SkillData.CharacterName is "Gustave" or "Verso" && node is { IsSecret: true, IsStarting: false })
        {
            node.IsSecret = false;
            node.RequiredItem = "null";
        }

        if (RandomizerLogic.Settings.MakeSkillsIntoItems)
        {
            node.IsSecret = !node.IsUnlockedByDefault;
            node.IsStarting = true;
            node.UnlockCost = 0;
            node.RequiredItem = node.IsUnlockedByDefault ? "null" : SkillsController.GetSkillUnlockItemName(node.SkillData);
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