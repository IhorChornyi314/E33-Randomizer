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

    public static List<string> DuelEncounters = [];

    private static ObjectPool<EnemyData> _bossPool;
    private static ObjectPool<ItemData> _keyItemsPool, _gearItemsPool;
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
        var bannedBosses = Controllers.EnemiesController.GetObjects(RandomizerLogic.CustomEnemyPlacement.ExcludedCodeNames);
        if (!RandomizerLogic.Settings.IncludeCutContentEnemies)
        {
            bannedBosses.AddRange(Controllers.EnemiesController.ObjectsData.Where(e => e.CustomName.Contains("Cut")).ToList());
        }

        _bossPool = new ObjectPool<EnemyData>(Controllers.EnemiesController.ObjectsData, bannedBosses);
        _keyItemsPool = new ObjectPool<ItemData>(
            Controllers.ItemsController.GetObjects(RandomizerLogic.CustomItemPlacement.PlainNameToCodeNames["Key Item"]),
            Controllers.ItemsController.GetObjects(RandomizerLogic.CustomItemPlacement.ExcludedCodeNames)
            );
        var gearItems = new List<ItemData>(Controllers.ItemsController.GetObjects(RandomizerLogic.CustomItemPlacement.PlainNameToCodeNames["Weapon"]));
        gearItems.AddRange(Controllers.ItemsController.GetObjects(RandomizerLogic.CustomItemPlacement.PlainNameToCodeNames["Pictos"]));
        
        _gearItemsPool = new ObjectPool<ItemData>(gearItems, Controllers.ItemsController.GetObjects(RandomizerLogic.CustomItemPlacement.ExcludedCodeNames));
    }

    public static void ResetSkillsPool()
    {
        _skillCategoryPools = new Dictionary<string, ObjectPool<SkillData>>();
    }
    
    public static void ApplySimonSpecialRule(Encounter encounter)
    {
        for (int i = 0; i < encounter.Size; i++)
        {
            if (encounter.Enemies[i].CodeName == "Boss_Simon_Phase2")
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
        
        if (RandomizerLogic.Settings.NoSimonP2BeforeLune && MandatoryEncounters.Contains(encounter.Name) && MandatoryEncounters.IndexOf(encounter.Name) < 5)
        {
            ApplySimonSpecialRule(encounter);
        }

        if (encounter.Name == "MM_DanseuseAlphaSummon")
        {
            encounter.Enemies = [Controllers.EnemiesController.GetObject("MM_Danseuse_CloneAlpha"), Controllers.EnemiesController.GetObject("MM_Danseuse_CloneAlpha")];
        }
        
        if (encounter.Name == "MM_DanseuseClone*1")
        {
            encounter.Enemies = [Controllers.EnemiesController.GetObject("MM_Danseuse_Clone")];
        }
        
        if (encounter.Name == "QUEST_Danseuse_DanceClass_Clone*1")
        {
            encounter.Enemies = [Controllers.EnemiesController.GetObject("MM_Danseuse_Clone")];
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

        if (encounter.Name != "Boss_Duolliste_P2")
        {
            encounter.Enemies = encounter.Enemies.Select(e => e.CodeName == "Duolliste_P2" ? Controllers.EnemiesController.GetObject("Duolliste_A"): e).ToList();
        }
    }

    public static void ApplySpecialRulesToCheck(CheckData check)
    {
        if (RandomizerLogic.Settings.EnsurePaintedPowerFromPaintress &&
            check.ItemSource.FileName == "DA_GA_SQT_RedAndWhiteTree")
        {
            check.ItemSource.AddItem("BP_GameAction_AddItemToInventory_C_0", Controllers.ItemsController.GetObject("OverPowered"));
        }

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
        
        if (RandomizerLogic.Settings.RandomizeStartingWeapons && check.Key.Contains("Chest_Generic_Chroma"))
        {
            var randomWeapon = Controllers.ItemsController.GetRandomWeapon("Gustave");

            check.ItemSource.SourceSections["Chest_Generic_Chroma"].Add(new ItemSourceParticle(randomWeapon));
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
            _skillCategoryPools[replacedSkillCategory] = new ObjectPool<SkillData>(possibleReplacements, []);
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
                    _skillCategoryPools["Default"] = new ObjectPool<SkillData>(Controllers.SkillsController.GetObjects(defaultReplacements), []);
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