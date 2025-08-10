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
    
    public override void InitPlainNames()
    {
        CategoryOrder = new List<string>
        {
            "Merchants", "Mimes", "Cut Content Enemies", "Gimmick/Tutorial Enemies",
            "Petanks", "Giant Enemies/Bosses", "Regular", "Weak Regular", "Elusive Regular", "Strong Regular", 
            "Minibosses", "Non-Chromatic Bosses", "Chromatic Bosses", "All Regular Enemies",
            "Main Plot Bosses", "Side Bosses", "All Bosses", "All Bosses and Minibosses", "Anyone"
        };
        
        using (StreamReader r = new StreamReader("Data/enemy_categories.json"))
        {
            string json = r.ReadToEnd();
            var customCategoryTranslationsString = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            PlainNameToCodeNames = customCategoryTranslationsString;
            CustomCategories = customCategoryTranslationsString.Keys.ToList();
        }


        PlainNameToCodeNames["Anyone"] = EnemiesController.enemies.Select(e => e.CodeName).ToList();
        PlainNamesList = ["Anyone"];
        
        PlainNamesList.AddRange(CustomCategories);
        PlainNamesList.InsertRange(PlainNamesList.IndexOf("All Regular Enemies") + 1, ArchetypeNames.Keys);

        foreach (var archetypeName in ArchetypeNames)
        {
            var allByArchetype = EnemiesController.GetAllByArchetype(archetypeName.Value);
            PlainNameToCodeNames[archetypeName.Key] = allByArchetype.Select(e => e.CodeName).ToList();
        }
        
        foreach (var enemyData in EnemiesController.enemies)
        {
            PlainNamesList.Add(enemyData.CustomName);
            PlainNameToCodeNames[enemyData.CustomName] = [enemyData.CodeName];
        }
        
        LoadDefaultPreset();
    }
    
    public override void LoadDefaultPreset()
    {
        NotRandomized = [];
        Excluded = ["Gimmick/Tutorial Enemies"];
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
    
    public override void UpdateDefaultFrequencies(Dictionary<string, float> translatedFrequencyAdjustments)
    {
        DefaultFrequencies = EnemiesController.enemies.Select(e => new KeyValuePair<string,float>(e.CodeName, translatedFrequencyAdjustments.ContainsKey(e.CodeName) ?  translatedFrequencyAdjustments[e.CodeName] : 1)).ToDictionary(kv => kv.Key, kv => kv.Value);
        DefaultFrequencies = DefaultFrequencies.Where(kv => kv.Value > 0.0001).ToDictionary();
    }
    
    public override string Replace(string originalCodeName)
    {
        if (NotRandomizedCodeNames.Contains(originalCodeName))
        {
            return originalCodeName;
        }

        if (!FinalReplacementFrequencies.TryGetValue(originalCodeName, out var frequency))
            return RandomizerLogic.GetRandomEnemy().CodeName;
        
        var newEnemy = Utils.GetRandomWeighted(
            frequency,
            ExcludedCodeNames
        );
        
        return newEnemy != null ? newEnemy : originalCodeName;
    }
}