using System.IO;
using Newtonsoft.Json;

namespace E33Randomizer;

public class CustomSkillPlacement: CustomPlacement
{
    public override void Init()
    {
        AllObjects = Controllers.SkillsController.ObjectsData;
        CatchAllName = "Anything";
        CategoryOrder = new List<string>
        {
            "Gustave", "Lune", "Maelle", "Monoco", "Verso", "Sciel", "Julie", "Consumables", "Character Skills", "Anything"
        };
        
        PresetFiles = new()
        {
            {"Split categories (default)", "Data/presets/skills/default.json"},
            {"Total randomness", "Data/presets/skills/total_random.json"},
            {"Don't change gradients", "Data/presets/skills/non_gradient_only.json"},
            {"Feet for everyone", "Data/presets/skills/feet.json"},
            {"Custom preset 1", "Data/presets/skills/custom_1.json"},
            {"Custom preset 2", "Data/presets/skills/custom_2.json"},
        };
        
        LoadCategories($"{RandomizerLogic.DataDirectory}/skill_categories.json");
        
        LoadDefaultPreset();
    }

    public override void LoadDefaultPreset()
    {
        ResetRules();
        AddNotRandomized("Consumables");
        CustomPlacementRules = new Dictionary<string, Dictionary<string, float>>
        {
            { "Gustave", new Dictionary<string, float> { { "Gustave", 1 } } },
            { "Lune", new Dictionary<string, float> { { "Lune", 1 } } },
            { "Maelle", new Dictionary<string, float> { { "Maelle", 1 } } },
            { "Monoco", new Dictionary<string, float> { { "Monoco", 1 } } },
            { "Verso", new Dictionary<string, float> { { "Verso", 1 } } },
            { "Sciel", new Dictionary<string, float> { { "Sciel", 1 } } },
        };
        FrequencyAdjustments = new Dictionary<string, float>();
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }
}