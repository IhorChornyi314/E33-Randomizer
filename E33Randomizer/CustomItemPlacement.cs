using System.IO;
using Newtonsoft.Json;

namespace E33Randomizer;

public class CustomItemPlacement: CustomPlacement
{
    public Dictionary<string, string> ItemCategories = new();
    public override void InitPlainNames()
    {
        CatchAllName = "Anything";
        CategoryOrder = new List<string>
        {
            "Pictos", "Non-functional", "Weapon", "Key Item", "Skill Unlock", "Cosmetic", "Upgrade Material", "Consumable", "Merchant Unlock", "Music Record", "Lovely Foot", "Journal", "Cut Content", "Anything"
        };
        
        using (StreamReader r = new StreamReader($"{RandomizerLogic.DataDirectory}/item_categories.json"))
        {
            string json = r.ReadToEnd();
            var customCategoryTranslationsString = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);

            foreach (var categoryTranslation in customCategoryTranslationsString)
            {
                foreach (var itemCodeName in categoryTranslation.Value)
                {
                    ItemCategories[itemCodeName] = categoryTranslation.Key;
                }
            }
            
            PlainNameToCodeNames = customCategoryTranslationsString;
            CustomCategories = customCategoryTranslationsString.Keys.ToList();
        }
        
        PlainNameToCodeNames["Anything"] = ItemsController.ItemsData.Select(i => i.CodeName).ToList();
        PlainNamesList = ["Anything"];
        
        PlainNamesList.AddRange(CustomCategories);
        
        foreach (var itemData in ItemsController.ItemsData)
        {
            PlainNamesList.Add(itemData.CustomName);
            PlainNameToCodeNames[itemData.CustomName] = [itemData.CodeName];
        }
        
        PresetFiles = new()
        {
            {"Split categories (default)", "Data/presets/items/default.json"},
            {"Total randomness", "Data/presets/items/total_random.json"},
            {"Only change pictos and weapons", "Data/presets/items/pictos_weapons_only.json"},
            {"Add more pictos", "Data/presets/items/more_pictos.json"},
            {"Custom preset 1", "Data/presets/items/custom_1.json"},
            {"Custom preset 2", "Data/presets/items/custom_2.json"},
        };
        
        LoadDefaultPreset();
    }

    public override void LoadDefaultPreset()
    {
        AddNotRandomized("Skill Unlock");
        AddNotRandomized("Merchant Unlock");
        AddExcluded("Non-functional");
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
        FrequencyAdjustments = new Dictionary<string, float>();
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }

    public override void UpdateDefaultFrequencies(Dictionary<string, float> translatedFrequencyAdjustments)
    {
        DefaultFrequencies = ItemsController.ItemsData.Select(i => new KeyValuePair<string,float>(i.CodeName, translatedFrequencyAdjustments.GetValueOrDefault(i.CodeName, 1))).ToDictionary(kv => kv.Key, kv => kv.Value);
        DefaultFrequencies = DefaultFrequencies.Where(kv => kv.Value > 0.0001).ToDictionary();
    }

    public override string Replace(string originalCodeName)
    {
        if (NotRandomizedCodeNames.Contains(originalCodeName))
        {
            return originalCodeName;
        }

        if (!FinalReplacementFrequencies.TryGetValue(originalCodeName, out var frequency))
            return RandomizerLogic.GetRandomItem().CodeName;
        
        var newItem = Utils.GetRandomWeighted(
            frequency,
            ExcludedCodeNames
        );
        
        return newItem != null ? newItem : originalCodeName;
    }
}