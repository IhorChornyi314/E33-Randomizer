using System.IO;
using Newtonsoft.Json;

namespace E33Randomizer;

public class CustomSkillPlacement: CustomPlacement
{
    public override void InitPlainNames()
    {
        CatchAllName = "Anything";
        CategoryOrder = new List<string>
        {
            "Gustave", "Julie", "Lune", "Maelle", "Monoco", "Verso", "Sciel", "Consumables", "Character Skills", "Anything"
        };
        
        using (StreamReader r = new StreamReader($"{RandomizerLogic.DataDirectory}/skill_categories.json"))
        {
            string json = r.ReadToEnd();
            var customCategoryTranslationsString = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            
            PlainNameToCodeNames = customCategoryTranslationsString;
            CustomCategories = customCategoryTranslationsString.Keys.ToList();
        }
        
        PlainNameToCodeNames["Anything"] = Controllers.SkillsController.ObjectsData.Select(o => o.CodeName).ToList();
        PlainNamesList = ["Anything"];
        
        PlainNamesList.AddRange(CustomCategories);
        
        foreach (var skillData in Controllers.SkillsController.ObjectsData)
        {
            PlainNamesList.Add(skillData.CustomName);
            PlainNameToCodeNames[skillData.CustomName] = [skillData.CodeName];
        }
        
        PresetFiles = new()
        {
            {"Split categories (default)", "Data/presets/skills/default.json"},
            {"Total randomness", "Data/presets/skills/total_random.json"},
            {"Don't change gradients", "Data/presets/skills/non_gradient_only.json"},
            {"Feet for everyone", "Data/presets/skills/feet.json"},
            {"Custom preset 1", "Data/presets/skills/custom_1.json"},
            {"Custom preset 2", "Data/presets/skills/custom_2.json"},
        };
        
        LoadDefaultPreset();
    }

    public override void LoadDefaultPreset()
    {
        throw new NotImplementedException();
    }

    public override void UpdateDefaultFrequencies(Dictionary<string, float> translatedFrequencyAdjustments)
    {
        throw new NotImplementedException();
    }

    public override string GetTrulyRandom()
    {
        return Controllers.SkillsController.GetRandomObject().CodeName;
    }
}