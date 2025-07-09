using System.IO;
using Newtonsoft.Json;

namespace E33Randomizer;

/*
 * Priority list:
 * Individual Enemy
 * Mimes/Merchants/Cut Content/Gimmick/Petank
 * Giant Enemies
 * Weak/Elusive/Regular/Strong/Elite/Alpha Enemies
 * All Regular Enemies
 * Main Plot/Side Bosses
 * All Bosses
 * All Bosses and Minibosses
 * Everyone
 */


class CustomEnemyPlacementPreset
{
    public List<String> NotRandomized = new List<String>();
    public List<String> Excluded = new List<String>();
    public Dictionary<String, Dictionary<String, float>> CustomPlacement = new Dictionary<string, Dictionary<string, float>>();
    public Dictionary<String, float> FrequencyAdjustments = new Dictionary<string, float>();

    public CustomEnemyPlacementPreset(List<String> n, List<String> e, Dictionary<String, Dictionary<String, float>> c,
        Dictionary<String, float> f)
    {
        NotRandomized = n;
        Excluded = e;
        CustomPlacement = c;
        FrequencyAdjustments = f;
    }
}

public static class CustomEnemyPlacement
{
    public static List<String> NotRandomized = new List<String>();
    public static List<String> Excluded = new List<String>();
    public static List<EnemyData> NotRandomizedTranslated => TranslatePlacementOptions(NotRandomized);
    public static List<EnemyData> ExcludedTranslated => TranslatePlacementOptions(Excluded);
    public static List<String> PlacementOptionsList = new List<string>();
    public static List<String> CustomCategories = new List<string>()
    {
        "Anyone",
        "All Bosses and Minibosses",
        "All Bosses",
        "Main Plot Bosses",
        "Side Bosses",
        "All Regular Enemies",
        "Giant Enemies/Bosses",
        "Merchants",
        "Mimes",
        "Cut Content Enemies",
        "Gimmick/Tutorial Enemies"
    };
    public static Dictionary<String, String> ArchetypeNames = new Dictionary<string, string>()
    {
        { "Regular", "Regular" },
        { "Weak Regular", "Weak"},
        { "Elusive Regular", "Elusive" },
        { "Strong Regular", "Strong" },
        { "Minibosses", "Elite" },
        { "Non-Chromatic Bosses", "Boss" },
        { "Chromatic Bosses", "Alpha" },
        { "Petanks", "Petank" },
    };
    public static Dictionary<String, Dictionary<String, float>> CustomPlacement = new Dictionary<string, Dictionary<string, float>>();
    public static Dictionary<String, float> FrequencyAdjustments = new Dictionary<string, float>();
    public static Dictionary<String, Dictionary<String, float>> FinalEnemyReplacementFrequencies =
        new Dictionary<string, Dictionary<string, float>>();
    
    public static void InitPlacementOptions()
    {
        PlacementOptionsList = new List<string>();
        PlacementOptionsList.AddRange(CustomCategories);
        PlacementOptionsList.InsertRange(PlacementOptionsList.IndexOf("All Regular Enemies"), ArchetypeNames.Keys);
        foreach (var enemyData in RandomizerLogic.allEnemies)
        {
            PlacementOptionsList.Add(enemyData.Name);
        }
        LoadDefaultPreset();
    }

    public static void LoadDefaultPreset()
    {
        NotRandomized = new List<string>();
        Excluded = new List<string> {"Gimmick/Tutorial Enemies"};
        CustomPlacement = new Dictionary<string, Dictionary<string, float>>
        {
            { "Regular", new Dictionary<string, float> { { "Regular", 1 } } },
            { "Weak Regular", new Dictionary<string, float> { { "Weak Regular", 1 } } },
            { "Elusive Regular", new Dictionary<string, float> { { "Elusive Regular", 1 } } },
            { "Strong Regular", new Dictionary<string, float> { { "Strong Regular", 1 } } },
            { "Minibosses", new Dictionary<string, float> { { "Minibosses", 1 } } },
            { "Non-Chromatic Bosses", new Dictionary<string, float> { { "Non-Chromatic Bosses", 1 } } },
            { "Chromatic Bosses", new Dictionary<string, float> { { "Chromatic Bosses", 1 } } },
            { "Petanks", new Dictionary<string, float> { { "Petanks", 1 } } },
        };
        FrequencyAdjustments = new Dictionary<string, float>
        {
            { "Cut Content Enemies", 0.2f }
        };
        FinalEnemyReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
        UpdateFinalEnemyReplacementFrequencies();
    }

    public static void LoadFromJson(String pathToJson)
    {
        using (StreamReader r = new StreamReader(pathToJson))
        {
            string json = r.ReadToEnd();
            var presetData = JsonConvert.DeserializeObject<CustomEnemyPlacementPreset>(json);
            NotRandomized = presetData.NotRandomized;
            Excluded = presetData.Excluded;
            CustomPlacement = presetData.CustomPlacement;
            FrequencyAdjustments = presetData.FrequencyAdjustments;
        }
        UpdateFinalEnemyReplacementFrequencies();
    }

    public static void SetCustomPlacement(String from, String to, float frequency)
    {
        if (!CustomPlacement.ContainsKey(from))
        {
            return;
        }

        CustomPlacement[from][to] = frequency;
        UpdateFinalEnemyReplacementFrequencies();
    }

    public static void RemoveCustomEnemyPlacement(String from, String to)
    {
        if (!CustomPlacement.ContainsKey(from) || !CustomPlacement[from].ContainsKey(to))
        {
            return;
        }

        CustomPlacement[from].Remove(to);
        UpdateFinalEnemyReplacementFrequencies();
    }
    
    public static void SaveToJson(String pathToJson)
    {
        using (StreamWriter r = new StreamWriter(pathToJson)) 
        {
            var presetData = new CustomEnemyPlacementPreset(NotRandomized, Excluded, CustomPlacement, FrequencyAdjustments);
            String json = JsonConvert.SerializeObject(presetData);
            r.Write(json);
        }
    }
    
    public static List<EnemyData> TranslatePlacementOption(String option)
    {
        if (option == "Anyone")
        {
            return RandomizerLogic.allEnemies;
        }
        
        if (CustomCategories.Contains(option))
        {
            var result = new List<EnemyData>();
            switch (option)
            {
                case "All Bosses":
                    result = new List<EnemyData>();
                    result.AddRange(RandomizerLogic.GetAllByArchetype("Boss"));
                    result.AddRange(RandomizerLogic.GetAllByArchetype("Alpha"));
                    return result;
                case "All Bosses and Minibosses":
                    result = new List<EnemyData>();
                    result.AddRange(RandomizerLogic.GetAllByArchetype("Boss"));
                    result.AddRange(RandomizerLogic.GetAllByArchetype("Alpha"));
                    result.AddRange(RandomizerLogic.GetAllByArchetype("Elite"));
                    return result;
                case "Giant Enemies/Bosses":
                    return RandomizerLogic.GetEnemyDataList(new List<string>()
                    {
                        "MM_Gargant",
                        "ML_Gargant_Gold",
                        "CFH_Gargant",
                        "SI_Glissando",
                        "SI_Glissando_Alpha",
                        "MO_Boss_Paintress",
                        "SI_Axon_Sirene",
                        "MF_Axon_Visages",
                        "SL_Sapling_CrushingWall",
                        "WM_Serpenphare",
                    });
                case "Main Plot Bosses":
                    return RandomizerLogic.GetEnemyDataList(new List<string>()
                    {
                        "SM_Boss_Eveque",
                        "ML_Eveque_Gold",
                        "SM_Boss_EvequeLuneScript",
                        "AS_PotatoBag_Boss",
                        "FB_DuallisteLR",
                        "GDC_Dualliste",
                        "MM_MirrorRenoir",
                        "MO_Boss_Paintress",
                        "SI_Axon_Sirene",
                        "MF_Axon_Visages",
                        "GO_Goblu_CleaTower",
                        "GO_Goblu",
                        "CFH_Goblu",
                        "MS_Monoco",
                        "L_Boss_Curator",
                        "FB_Dualliste_Phase2",
                        "FinalBoss_Verso",
                        "FinalBoss_Maelle",
                        "ML_PaintressIntro",
                        "MF_Axon_MaskKeeper_VisagesPhase2",
                        "OL_MirrorRenoir_FirstFight",
                        "SC_LampMaster",
                        "TS_PotatoBag_Boss_Twilight",
                        "CFH_Dualliste",
                        "CFH_LampMaster",
                        "CT_MaskKeeper_NoMask",
                        "QUEST_MonocoDuel",
                        "CW_LampMasterAlpha",
                        "SC_LampMaster_CleasTower",
                        "FB_Dualliste_CleasTower",
                        "CT_MaskKeeper_NoMask_CleaTwoer",
                        "ML_PotatoBag_Boss_Gold",
                        "SM_Boss_Eveque_ShieldTutorial",
                        "AS_PotatoBag_Boss_Quest",
                    });
                case "Side Bosses":
                    return RandomizerLogic.GetEnemyDataList(new List<string>()
                    {
                        "YF_Boss_Scavenger",
                        "MM_Gargant",
                        "SI_Tisseur",
                        "RC_Alicia",
                        "WM_Serpenphare",
                        "WM_Sprong",
                        "QUEST_Golgra_DarkArena",
                        "QUEST_Golgra_SacredRiver",
                        "YF_Glaise_Boss",
                        "Boss_Simon",
                        "CFH_Boss_Clea",
                        "ML_Gargant_Gold",
                        "GV_Golgra",
                        "Boss_Simon_Phase2",
                        "CT_Boss_Curator_CleaTower",
                        "CT_Boss_Paintress_CleaTower",
                        "CFH_Gargant",
                    });
                case "All Regular Enemies":
                    result = new List<EnemyData>();
                    result.AddRange(RandomizerLogic.GetAllByArchetype("Weak"));
                    result.AddRange(RandomizerLogic.GetAllByArchetype("Elusive"));
                    result.AddRange(RandomizerLogic.GetAllByArchetype("Regular"));
                    result.AddRange(RandomizerLogic.GetAllByArchetype("Strong"));
                    return result;
                case "Merchants":
                    return RandomizerLogic.allEnemies.FindAll(e => e.CodeName.Contains("Merchant_"));
                case "Mimes":
                    return RandomizerLogic.allEnemies.FindAll(e => e.CodeName.Contains("Mime"));
                case "Cut Content Enemies":
                    result = RandomizerLogic.GetEnemyDataList(new List<String>()
                    {
                        "YF_GaultA",
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
                        "NevronWall",
                    });
                    result.AddRange(RandomizerLogic.allEnemies.FindAll(e => e.CodeName.Contains("Alternati")));
                    result.AddRange(RandomizerLogic.allEnemies.FindAll(e => e.CodeName.Contains("CZ_Chroma")));
                    return result;
                case "Gimmick/Tutorial Enemies":
                    return RandomizerLogic.GetEnemyDataList(new List<string>()
                    {
                        "LU_Act1_Sophie",
                        "CAMP_PunchingBall",
                        "Quest_GestralSumo9999",
                        "LU_Act1_PunchingBall",
                        "L_MaelleTutorial_NoTutorial_Civilian",
                        "L_MaelleTutorial_Civilian",
                        "L_MaelleTutorial",
                        "L_MaelleTutorial_NoTutorial",
                        "SC_MirrorRenoir_GustaveEnd",
                        "ML_PaintressIntro",
                        "FinalBoss_Verso",
                        "FinalBoss_Maelle",
                        "QUEST_TroubadourCantPlay",
                        "QUEST_Danseuse_DanceClass_Kill",
                        "QUEST_Danseuse_DanceClass_Clone",
                        "SL_Sapling_CrushingWall",
                        "GO_Curator_JumpTutorial",
                        "GO_Curator_JumpTutorial_NoTuto",
                    });
            }
        }
        
        if (ArchetypeNames.ContainsKey(option))
        {
            return RandomizerLogic.GetAllByArchetype(ArchetypeNames[option]);
        }

        if (RandomizerLogic.allEnemies.Exists(e => e.Name == option))
        {
            return RandomizerLogic.allEnemies.FindAll(e => e.Name == option);
        }

        return new List<EnemyData>();
    }

    public static List<EnemyData> TranslatePlacementOptions(List<String> options)
    {
        var result = new List<EnemyData>();
        foreach (var option in options)
        {
            result.AddRange(TranslatePlacementOption(option));
        }

        return result;
    }
    
    public static Dictionary<String, T> CustomCategoryDictionaryToCodeNames<T>(Dictionary<String, T> from)
    {
        Dictionary<String, T> result = new Dictionary<string, T>();
        foreach (var pair in from)
        {
            var translatedKey = TranslatePlacementOption(pair.Key);
            foreach (var enemyData in translatedKey)
            {
                result[enemyData.CodeName] = pair.Value;
            }
        }

        return result;
    }

    public static void UpdateFinalEnemyReplacementFrequencies()
    {
        FinalEnemyReplacementFrequencies.Clear();
        //CustomPlacement keys are plain text - NOT codenames
        var individualEnemyPlacements = CustomPlacement.Where(kvp =>
            RandomizerLogic.allEnemies.Contains(RandomizerLogic.GetEnemyDataByName(kvp.Key)));
        var merchantsMimesCutGimmickPetanks = CustomPlacement.Where(kvp =>
            kvp.Key == "Merchants" ||
            kvp.Key == "Mimes" ||
            kvp.Key == "Cut Content Enemies" ||
            kvp.Key == "Gimmick/Tutorial Enemies" ||
            kvp.Key == "Petanks"
            );
        var giantEnemies = CustomPlacement.Where(kvp =>
            kvp.Key == "Giant Enemies/Bosses");
        var archetypeEnemies = CustomPlacement.Where(kvp => ArchetypeNames.Keys.Contains(kvp.Key));
        var allRegularEnemies = CustomPlacement.Where(kvp =>
            kvp.Key == "All Regular Enemies");
        var mainSideBosses = CustomPlacement.Where(kvp =>
            kvp.Key == "Main Plot Bosses" ||
            kvp.Key == "Side Bosses"
        );
        var allBosses = CustomPlacement.Where(kvp =>
            kvp.Key == "All Bosses");
        var allBossesMinibosses = CustomPlacement.Where(kvp =>
            kvp.Key == "All Bosses and Minibosses");
        var anyone = CustomPlacement.Where(kvp =>
            kvp.Key == "Anyone");
        //This is a monstrosity the likes of which the world has never yet seen
        foreach (var enemyGroup in new List<IEnumerable<KeyValuePair<String, Dictionary<String, float>>>>()
                 {
                     individualEnemyPlacements, merchantsMimesCutGimmickPetanks, giantEnemies, archetypeEnemies,
                     allRegularEnemies, mainSideBosses, allBosses, allBossesMinibosses, anyone
                 })
        {
            foreach (var enemy in enemyGroup)
            {
                var translatedEnemies = TranslatePlacementOption(enemy.Key);
                foreach (var translatedEnemy in translatedEnemies)
                {
                    if (FinalEnemyReplacementFrequencies.ContainsKey(translatedEnemy.CodeName))
                    {
                        continue;
                    }
                    FinalEnemyReplacementFrequencies[translatedEnemy.CodeName] = CustomCategoryDictionaryToCodeNames(enemy.Value);
                    
                    //Account for frequency adjustments
                    foreach (var replacementWeight in FinalEnemyReplacementFrequencies[translatedEnemy.CodeName])
                    {
                        if (FrequencyAdjustments.ContainsKey(replacementWeight.Key))
                        {
                            FinalEnemyReplacementFrequencies[translatedEnemy.CodeName][replacementWeight.Key] *= FrequencyAdjustments[replacementWeight.Key];
                        }
                    }
                }
            }
        }
    }
    
    public static EnemyData Replace(EnemyData original)
    {
        if (NotRandomizedTranslated.Contains(original))
        {
            return original;
        }

        if (FinalEnemyReplacementFrequencies.ContainsKey(original.CodeName))
        {
            var bannedEnemies = TranslatePlacementOptions(Excluded);
            var bannedEnemyNames = bannedEnemies.Select(e => e.CodeName);
            
            return RandomizerLogic.GetEnemyData(
                Utils.GetRandomWeighted(
                    FinalEnemyReplacementFrequencies[original.CodeName], 
                    bannedEnemyNames.ToList()
                    )
                );
        }
        
        return RandomizerLogic.GetRandomEnemy();
    }
}