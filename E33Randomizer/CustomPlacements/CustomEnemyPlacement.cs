using E33Randomizer.RadomizationLogic;

namespace E33Randomizer.CustomPlacements;

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

public class CustomEnemyPlacement : CustomPlacementWindowViewModel
{
    public override string EntityType => "Enemy";

    private readonly Dictionary<string, string> _archetypeNames = new()
    {
        { "Regular", "Regular" },
        { "Weak Regular", "Weak" },
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
            "Main Plot Bosses", "Side Bosses", "Super Bosses", "All Bosses", "All Bosses and Minibosses", "DLC Enemies",
            "Anyone"
        };

        CategoryDisplayOrder =
        [
            "Anyone", "All Bosses and Minibosses", "All Bosses", "Main Plot Bosses", "Side Bosses", "Super Bosses", "All Regular Enemies",
            "Regular", "Weak Regular", "Elusive Regular", "Strong Regular", "Minibosses", "Non-Chromatic Bosses", "Chromatic Bosses",
            "Petanks", "Giant Enemies/Bosses", "Map Part Enemies", "Merchants", "Mimes", "Cut Content Enemies", "Gimmick/Tutorial Enemies",
            "DLC Enemies"
        ];

        PresetFiles = new()
        {
            { "Split categories (default)", "Data/Presets/enemies/default.json" },
            { "Total randomness", "Data/Presets/enemies/total_random.json" },
            { "10% of regular enemies are bosses", "Data/Presets/enemies/10_percent.json" },
            { "Make every enemy a boss", "Data/Presets/enemies/everyone_is_a_boss.json" },
            { "Custom preset 1", "Data/Presets/enemies/custom_1.json" },
            { "Custom preset 2", "Data/Presets/enemies/custom_2.json" },
        };

        AdditionalCategoriesToAdd = _archetypeNames.Keys.ToArray();
        
        LoadCategories($"{RandomizerLogic.DataDirectory}/enemy_categories.json");


        foreach (var archetypeName in _archetypeNames)
        {
            var allByArchetype = Controllers.EnemiesController.GetAllByArchetype(archetypeName.Value);
            PlainNameToCodeNames[archetypeName.Key] = allByArchetype.Select(e => e.CodeName).ToList();
        }

        LoadDefaultPreset();
    }

    public override void LoadDefaultPreset()
    {
        AddExcluded("Gimmick/Tutorial Enemies");
        AddExcluded("Map Part Enemies");
        CustomPlacementRules.AddRange(new Dictionary<string, IEnumerable<KeyValuePair<string, byte>>>
        {
            { "Regular", [new KeyValuePair<string, byte>("Regular", 100)] },
            { "Weak Regular", [new KeyValuePair<string, byte>("Weak Regular", 100)] },
            { "Elusive Regular", [new KeyValuePair<string, byte>("Elusive Regular", 100)] },
            { "Strong Regular", [new KeyValuePair<string, byte>("Strong Regular", 100)] },
            { "Minibosses", [new KeyValuePair<string, byte>("Minibosses", 100)] },
            { "Non-Chromatic Bosses", [new KeyValuePair<string, byte>("Non-Chromatic Bosses", 100)] },
            { "Chromatic Bosses", [new KeyValuePair<string, byte>("Chromatic Bosses", 100)] },
            { "Petanks", [new KeyValuePair<string, byte>("Petanks", 100)] }
        });
        FrequencyAdjustments.AddRange(new Dictionary<string, byte>
        {
            { "Cut Content Enemies", 20 },
            { "Mimes", 20 }
        });
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }
}