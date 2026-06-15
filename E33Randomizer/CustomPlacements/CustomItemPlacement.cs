using E33Randomizer.RandomizationLogic;

namespace E33Randomizer.CustomPlacements;

public class CustomItemPlacement: CustomPlacementWindowViewModel
{
    public override string EntityType => "Item";

    public override void Init()
    {
        AllObjects = Controllers.ItemsController.ObjectsData.Where(i => i.Type != "CustomSkillUnlocker");
        CatchAllName = "Anything";
        CategoryOrder = new List<string>
        {
            "Pictos", "Weapon", "Key Item", "Skill Unlock", "Cosmetic", "Upgrade Material", "Consumable", "Merchant Unlock", "Music Record", "Lovely Foot", "Journal", "DLC Items", "Cut Content Items", "Anything"
        };
        
        PresetFiles = new()
        {
            {"Split categories (default)", "Data/Presets/items/default.json"},
            {"Total randomness", "Data/Presets/items/total_random.json"},
            {"Only change pictos and weapons", "Data/Presets/items/pictos_weapons_only.json"},
            {"Add more pictos", "Data/Presets/items/more_pictos.json"},
            {"Custom preset 1", "Data/Presets/items/custom_1.json"},
            {"Custom preset 2", "Data/Presets/items/custom_2.json"},
        };
        
        LoadCategories($"{RandomizerLogic.DataDirectory}/item_categories.json");
        
        LoadDefaultPreset();
    }

    public override void LoadDefaultPreset()
    {
        ResetRules();
        AddNotRandomized("Skill Unlock");
        AddNotRandomized("Merchant Unlock");
        AddExcluded("Consumable");
        CustomPlacementRules.AddRange(new Dictionary<string, IEnumerable<KeyValuePair<string, byte>>>
        {
            { "Pictos", [new KeyValuePair<string, byte>("Pictos", 100)] },
            { "Weapon", [new KeyValuePair<string, byte>("Weapon", 100)] },
            { "Key Item", [new KeyValuePair<string, byte>("Key Item", 100)] },
            { "Cosmetic", [new KeyValuePair<string, byte>("Cosmetic", 100)] },
            { "Upgrade Material", [new KeyValuePair<string, byte>("Upgrade Material", 100)] },
            { "Music Record", [new KeyValuePair<string, byte>("Music Record", 100)] }
        });
        FrequencyAdjustments.AddRange( new Dictionary<string, byte>{
            { "Cut Content Items", 50 },
            { "Revive Tint Shard (Upgrade Material)", 10 },
            { "Energy Tint Shard (Upgrade Material)", 10 },
            { "Healing Tint Shard (Upgrade Material)", 10 },
            { "Shape of Health (Upgrade Material)", 10 },
            { "Shape of Energy (Upgrade Material)", 10 },
            { "Shape of Life (Upgrade Material)", 10 },
            { "Chroma Elixir Shard (Upgrade Material)", 10 },
            { "Perfect Chroma Catalyst (Upgrade Material)", 40 }
        });
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }
}