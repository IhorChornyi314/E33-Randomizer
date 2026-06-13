using E33Randomizer.RandomizationLogic;

namespace E33Randomizer.CustomPlacements;

public class CustomSkillPlacement: CustomPlacementWindowViewModel
{
    public override string EntityType => "Skill";

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
            {"Split categories (default)", "Data/Presets/skills/default.json"},
            {"Total randomness", "Data/Presets/skills/total_random.json"},
            {"Don't change gradients", "Data/Presets/skills/non_gradient_only.json"},
            {"Feet for everyone", "Data/Presets/skills/feet.json"},
            {"Custom preset 1", "Data/Presets/skills/custom_1.json"},
            {"Custom preset 2", "Data/Presets/skills/custom_2.json"},
        };
        
        LoadCategories($"{RandomizerLogic.DataDirectory}/skill_categories.json");
        
        LoadDefaultPreset();
    }

    public override void LoadDefaultPreset()
    {
        ResetRules();
        AddNotRandomized("Consumables");
        CustomPlacementRules.AddRange(new Dictionary<string, IEnumerable<KeyValuePair<string, byte>>>
        {
            { "Gustave's Skills", [new KeyValuePair<string, byte>("Gustave's Skills", 100)] },
            { "Lune's Gradient Skills", [new KeyValuePair<string, byte>("Lune's Gradient Skills", 100)] },
            { "Lune's Non-gradient Skills", [new KeyValuePair<string, byte>("Lune's Non-gradient Skills", 100)] },
            { "Maelle's Gradient Skills", [new KeyValuePair<string, byte>("Maelle's Gradient Skills", 100)] },
            { "Maelle's Non-gradient Skills", [new KeyValuePair<string, byte>("Maelle's Non-gradient Skills", 100)] },
            { "Monoco's Gradient Skills", [new KeyValuePair<string, byte>("Monoco's Gradient Skills", 100)] },
            { "Monoco's Non-gradient Skills", [new KeyValuePair<string, byte>("Monoco's Non-gradient Skills", 100)] },
            { "Verso's Gradient Skills", [new KeyValuePair<string, byte>("Verso's Gradient Skills", 100)] },
            { "Verso's Non-gradient Skills", [new KeyValuePair<string, byte>("Verso's Non-gradient Skills", 100)] },
            { "Sciel's Gradient Skills", [new KeyValuePair<string, byte>("Sciel's Gradient Skills", 100)] },
            { "Sciel's Non-gradient Skills", [new KeyValuePair<string, byte>("Sciel's Non-gradient Skills", 100)] }
        });
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }
}