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


class CustomEnemyPlacementPreset(
    List<string> n,
    List<string> e,
    Dictionary<string, Dictionary<string, float>> c,
    Dictionary<string, float> f)
{
    public List<string> NotRandomized = n;
    public List<string> Excluded = e;
    public Dictionary<string, Dictionary<string, float>> CustomPlacement = c;
    public Dictionary<string, float> FrequencyAdjustments = f;
}

public static class CustomEnemyPlacement
{
    public static List<string> NotRandomized = [];
    public static List<string> Excluded = [];
    public static List<EnemyData> NotRandomizedTranslated => TranslatePlacementOptions(NotRandomized);
    public static List<EnemyData> ExcludedTranslated => TranslatePlacementOptions(Excluded);
    public static List<string> PlacementOptionsList = [];
    public static List<string> CustomCategories = new(); 
    public static Dictionary<string, string> ArchetypeNames = new()
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

    public static Dictionary<string, List<EnemyData>> CustomCategoryTranslations = new();
    public static Dictionary<string, Dictionary<string, float>> CustomPlacement = new();
    public static Dictionary<string, float> FrequencyAdjustments = new();
    public static Dictionary<string, float> DefaultFrequencies = new();
    public static Dictionary<string, Dictionary<string, float>> FinalEnemyReplacementFrequencies = new();
    public static Dictionary<string, float> TranslatedFrequencyAdjustments => CustomCategoryDictionaryToCodeNames(FrequencyAdjustments);
    public static List<string> BannedEnemyNames => TranslatePlacementOptions(Excluded).Select(e => e.CodeName).ToList();
    
    public static void InitPlacementOptions()
    {
        using (StreamReader r = new StreamReader("Data/enemy_categories.json"))
        {
            string json = r.ReadToEnd();
            var customCategoryTranslationsString = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            CustomCategoryTranslations =  customCategoryTranslationsString.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Select(EnemiesController.GetEnemyData).ToList()
            );
            CustomCategories = CustomCategoryTranslations.Keys.ToList();
        }
        
        PlacementOptionsList = ["Anyone"];
        PlacementOptionsList.AddRange(CustomCategories);
        PlacementOptionsList.InsertRange(PlacementOptionsList.IndexOf("All Regular Enemies"), ArchetypeNames.Keys);
        foreach (var enemyData in EnemiesController.enemies)
        {
            PlacementOptionsList.Add(enemyData.CustomName);
        }
        
        LoadDefaultPreset();
    }

    public static void LoadDefaultPreset()
    {
        NotRandomized = [];
        Excluded = ["Gimmick/Tutorial Enemies"];
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
    }

    public static void LoadFromJson(string pathToJson)
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
    }

    public static void SetCustomPlacement(string from, string to, float frequency)
    {
        if (!CustomPlacement.ContainsKey(from))
        {
            CustomPlacement[from] = new Dictionary<string, float>();
        }

        CustomPlacement[from][to] = frequency;
    }

    public static void RemoveCustomEnemyPlacement(string from, string to)
    {
        if (!CustomPlacement.ContainsKey(from) || !CustomPlacement[from].ContainsKey(to))
        {
            return;
        }

        CustomPlacement[from].Remove(to);
    }
    
    public static void SaveToJson(string pathToJson)
    {
        using StreamWriter r = new StreamWriter(pathToJson);
        var presetData = new CustomEnemyPlacementPreset(NotRandomized, Excluded, CustomPlacement, FrequencyAdjustments);
        string json = JsonConvert.SerializeObject(presetData);
        r.Write(json);
    }
    
    public static List<EnemyData> TranslatePlacementOption(string option)
    {
        if (option == "Anyone")
        {
            return EnemiesController.enemies;
        }
        
        if (CustomCategories.Contains(option))
        {
            return CustomCategoryTranslations[option];
        }
        
        if (ArchetypeNames.ContainsKey(option))
        {
            return EnemiesController.GetAllByArchetype(ArchetypeNames[option]);
        }

        if (EnemiesController.enemies.Exists(e => e.CustomName == option))
        {
            return EnemiesController.enemies.FindAll(e => e.CustomName == option);
        }

        return [];
    }

    public static List<EnemyData> TranslatePlacementOptions(List<string> options)
    {
        var result = new List<EnemyData>();
        foreach (var option in options)
        {
            result.AddRange(TranslatePlacementOption(option));
        }

        return result;
    }
    
    public static Dictionary<string, T> CustomCategoryDictionaryToCodeNames<T>(Dictionary<string, T> from)
    {
        Dictionary<string, T> result = new Dictionary<string, T>();
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

    public static void Update()
    {
        FinalEnemyReplacementFrequencies.Clear();
        var categoryOrder = new List<string>
        {
            "Merchants", "Mimes", "Cut Content Enemies", "Gimmick/Tutorial Enemies",
            "Petanks", "Giant Enemies/Bosses", "Regular", "Weak Regular", "Elusive Regular", "Strong Regular", 
            "Minibosses", "Non-Chromatic Bosses", "Chromatic Bosses", "All Regular Enemies",
            "Main Plot Bosses", "Side Bosses", "All Bosses", "All Bosses and Minibosses", "Anyone"
        };
        var orderedCustomPlacementKeys = CustomPlacement.Keys.OrderBy(k => categoryOrder.IndexOf(k));
        var translatedFrequencyAdjustments = TranslatedFrequencyAdjustments;
        foreach (var customPlacementKey in orderedCustomPlacementKeys)
        {
            var translatedKey = TranslatePlacementOption(customPlacementKey).Select(e => e.CodeName);
            foreach (var enemyCodeName in translatedKey)
            {
                if (FinalEnemyReplacementFrequencies.ContainsKey(enemyCodeName))
                {
                    continue;
                }
                var unadjustedFrequencies = CustomCategoryDictionaryToCodeNames(CustomPlacement[customPlacementKey]);
                foreach (var frequency in unadjustedFrequencies)
                {
                    if (translatedFrequencyAdjustments.ContainsKey(frequency.Key))
                    {
                        unadjustedFrequencies[frequency.Key] *= translatedFrequencyAdjustments[frequency.Key];
                    }

                    if (unadjustedFrequencies[frequency.Key] < 0.0001)
                    {
                        unadjustedFrequencies.Remove(frequency.Key);
                    }
                }

                if (unadjustedFrequencies.Any())
                {
                    FinalEnemyReplacementFrequencies[enemyCodeName] = unadjustedFrequencies;
                }
            }
        }
        DefaultFrequencies = EnemiesController.enemies.Select(e => new KeyValuePair<string,float>(e.CodeName, translatedFrequencyAdjustments.ContainsKey(e.CodeName) ?  translatedFrequencyAdjustments[e.CodeName] : 1)).ToDictionary(kv => kv.Key, kv => kv.Value);
        DefaultFrequencies = DefaultFrequencies.Where(kv => kv.Value > 0.0001).ToDictionary();
    }
    
    public static EnemyData Replace(EnemyData original)
    {
        if (NotRandomizedTranslated.Contains(original))
        {
            return original;
        }
        var bannedEnemies = TranslatePlacementOptions(Excluded);
        var bannedEnemyNames = bannedEnemies.Select(e => e.CodeName);
        
        if (FinalEnemyReplacementFrequencies.ContainsKey(original.CodeName))
        {
            var newEnemy = Utils.GetRandomWeighted(
                FinalEnemyReplacementFrequencies[original.CodeName],
                bannedEnemyNames.ToList()
            );

            return newEnemy != null ? EnemiesController.GetEnemyData(newEnemy) : original;
        }
        
        return RandomizerLogic.GetRandomEnemy();
    }
}