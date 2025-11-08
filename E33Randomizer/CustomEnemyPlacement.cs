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


public class CustomEnemyPlacement: CustomPlacement
{
    public Dictionary<string, string> ArchetypeNames = new()
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
    
    public override void Init()
    {
        AllObjects = Controllers.EnemiesController.ObjectsData;
        CatchAllName = "Anyone";
        CategoryOrder = new List<string>
        {
            "Merchants", "Mimes", "Cut Content Enemies", "Gimmick/Tutorial Enemies",
            "Petanks", "Giant Enemies/Bosses", "Regular", "Weak Regular", "Elusive Regular", "Strong Regular", 
            "Minibosses", "Non-Chromatic Bosses", "Chromatic Bosses", "All Regular Enemies",
            "Main Plot Bosses", "Side Bosses", "All Bosses", "All Bosses and Minibosses", "Anyone"
        };
        
        PresetFiles = new()
        {
            {"Split categories (default)", "Data/presets/enemies/default.json"},
            {"Total randomness", "Data/presets/enemies/total_random.json"},
            {"10% of regular enemies are bosses", "Data/presets/enemies/10_percent.json"},
            {"Make every enemy a boss", "Data/presets/enemies/everyone_is_a_boss.json"},
            {"Custom preset 1", "Data/presets/enemies/custom_1.json"},
            {"Custom preset 2", "Data/presets/enemies/custom_2.json"},
        };
        
        LoadCategories($"{RandomizerLogic.DataDirectory}/enemy_categories.json");
        
        PlainNamesList.InsertRange(PlainNamesList.IndexOf("All Regular Enemies") + 1, ArchetypeNames.Keys);

        foreach (var archetypeName in ArchetypeNames)
        {
            var allByArchetype = Controllers.EnemiesController.GetAllByArchetype(archetypeName.Value);
            PlainNameToCodeNames[archetypeName.Key] = allByArchetype.Select(e => e.CodeName).ToList();
        }
        
        LoadDefaultPreset();
    }
    
    public override void LoadDefaultPreset()
    {
        NotRandomized = [];
        AddExcluded("Gimmick/Tutorial Enemies");
        AddExcluded("Map Part Enemies");
        CustomPlacementRules = new Dictionary<string, Dictionary<string, float>>
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
            { "Cut Content Enemies", 0.2f },
            { "Mimes", 0.2f }
        };
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }
}