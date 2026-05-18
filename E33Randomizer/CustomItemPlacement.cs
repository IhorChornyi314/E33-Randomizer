using System.Collections.ObjectModel;

namespace E33Randomizer;

public class CustomItemPlacement: CustomPlacementWindowViewModel
{
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
        CustomPlacementRules = new Dictionary<string, Dictionary<string, float>>
        {
            { "Pictos", new Dictionary<string, float> { { "Pictos", 1 } } },
            { "Weapon", new Dictionary<string, float> { { "Weapon", 1 } } },
            { "Key Item", new Dictionary<string, float> { { "Key Item", 1 } } },
            { "Cosmetic", new Dictionary<string, float> { { "Cosmetic", 1 } } },
            { "Upgrade Material", new Dictionary<string, float> { { "Upgrade Material", 1 } } },
            { "Music Record", new Dictionary<string, float> { { "Music Record", 1 } } },
        };
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