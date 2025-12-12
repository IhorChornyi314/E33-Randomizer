using System.IO;
using Newtonsoft.Json;

namespace E33Randomizer;

public class CustomItemPlacement: CustomPlacement
{
    public override void Init()
    {
        AllObjects = Controllers.ItemsController.ObjectsData;
        CatchAllName = "Anything";
        CategoryOrder = new List<string>
        {
            "Pictos", "Weapon", "Key Item", "Skill Unlock", "Cosmetic", "Upgrade Material", "Consumable", "Merchant Unlock", "Music Record", "Lovely Foot", "Journal", "DLC Items", "Cut Content Items", "Anything"
        };
        
        PresetFiles = new()
        {
            {"Split categories (default)", "Data/presets/items/default.json"},
            {"Total randomness", "Data/presets/items/total_random.json"},
            {"Only change pictos and weapons", "Data/presets/items/pictos_weapons_only.json"},
            {"Add more pictos", "Data/presets/items/more_pictos.json"},
            {"Custom preset 1", "Data/presets/items/custom_1.json"},
            {"Custom preset 2", "Data/presets/items/custom_2.json"},
        };
        
        LoadCategories($"{RandomizerLogic.DataDirectory}/item_categories.json");
        
        LoadDefaultPreset();
    }

    public override void LoadDefaultPreset()
    {
        ResetRules();
        AddNotRandomized("Skill Unlock");
        AddNotRandomized("Merchant Unlock");
        AddNotRandomized("DLC Items");
        AddExcluded("Consumable");
        AddExcluded("DLC Items");
        CustomPlacementRules = new Dictionary<string, Dictionary<string, float>>
        {
            { "Pictos", new Dictionary<string, float> { { "Pictos", 1 } } },
            { "Weapon", new Dictionary<string, float> { { "Weapon", 1 } } },
            { "Key Item", new Dictionary<string, float> { { "Key Item", 1 } } },
            { "Cosmetic", new Dictionary<string, float> { { "Cosmetic", 1 } } },
            { "Upgrade Material", new Dictionary<string, float> { { "Upgrade Material", 1 } } },
            { "Music Record", new Dictionary<string, float> { { "Music Record", 1 } } },
        };
        FrequencyAdjustments = new Dictionary<string, float>{
            { "Cut Content Items", 0.5f },
            { "Revive Tint Shard (Upgrade Material)", 0.1f },
            { "Energy Tint Shard (Upgrade Material)", 0.1f },
            { "Healing Tint Shard (Upgrade Material)", 0.1f },
            { "Shape of Health (Upgrade Material)", 0.1f },
            { "Shape of Energy (Upgrade Material)", 0.1f },
            { "Shape of Life (Upgrade Material)", 0.1f },
            { "Chroma Elixir Shard (Upgrade Material)", 0.1f },
            { "Perfect Chroma Catalyst (Upgrade Material)", 0.4f }
        };
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }
}