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
            "Lune's Gradient Skills", "Lune's Non-gradient Skills", "Maelle's Gradient Skills", "Maelle's Non-gradient Skills",
            "Monoco's Gradient Skills", "Monoco's Non-gradient Skills", "Verso's Gradient Skills", "Verso's Non-gradient Skills",
            "Sciel's Gradient Skills", "Sciel's Non-gradient Skills", "Gustave's Skills", "Lune's Skills", 
            "Maelle's Skills", "Monoco's Skills", "Verso's Skills", "Sciel's Skills", "Julie's Skills", "Consumables", 
            "Gradient Skills", "Non-gradient Skills", "Character Skills", "Cut Content Skills", "Anything"
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
            { "Gustave's Skills", new Dictionary<string, float> { { "Gustave's Skills", 1 } } },
            { "Lune's Gradient Skills", new Dictionary<string, float> { { "Lune's Gradient Skills", 1 } } },
            { "Lune's Non-gradient Skills", new Dictionary<string, float> { { "Lune's Non-gradient Skills", 1 } } },
            { "Maelle's Gradient Skills", new Dictionary<string, float> { { "Maelle's Gradient Skills", 1 } } },
            { "Maelle's Non-gradient Skills", new Dictionary<string, float> { { "Maelle's Non-gradient Skills", 1 } } },
            { "Monoco's Gradient Skills", new Dictionary<string, float> { { "Monoco's Gradient Skills", 1 } } },
            { "Monoco's Non-gradient Skills", new Dictionary<string, float> { { "Monoco's Non-gradient Skills", 1 } } },
            { "Verso's Gradient Skills", new Dictionary<string, float> { { "Verso's Gradient Skills", 1 } } },
            { "Verso's Non-gradient Skills", new Dictionary<string, float> { { "Verso's Non-gradient Skills", 1 } } },
            { "Sciel's Gradient Skills", new Dictionary<string, float> { { "Sciel's Gradient Skills", 1 } } },
            { "Sciel's Non-gradient Skills", new Dictionary<string, float> { { "Sciel's Non-gradient Skills", 1 } } },
        };
        FrequencyAdjustments = new Dictionary<string, float>();
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }
}