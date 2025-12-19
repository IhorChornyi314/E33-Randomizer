using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace E33Randomizer;

class CustomPlacementPreset(
    List<string> n,
    List<string> e,
    Dictionary<string, Dictionary<string, float>> c,
    Dictionary<string, float> f)
{
    public List<string> NotRandomized = n;
    public List<string> Excluded = e;
    public Dictionary<string, Dictionary<string, float>> CustomPlacement = c;
    public Dictionary<string, float> FrequencyAdjustments = f;
}

public abstract class CustomPlacement
{
    public List<string> NotRandomized = [];
    public List<string> Excluded = [];
    public List<string> NotRandomizedCodeNames = [];
    public List<string> ExcludedCodeNames = [];
    
    public List<string> PlainNamesList = [];
    public Dictionary<string, List<string>> PlainNameToCodeNames = new();
    
    public List<string> CustomCategories = new(); 
    public Dictionary<string, Dictionary<string, float>> CustomPlacementRules = new();
    public Dictionary<string, float> FrequencyAdjustments = new();
    public Dictionary<string, float> DefaultFrequencies = new();
    public Dictionary<string, Dictionary<string, float>> FinalReplacementFrequencies = new();
    public List<string> CategoryOrder = new();
    public IEnumerable<ObjectData> AllObjects;
    
    public Dictionary<string, string> PresetFiles = new();
    protected string CatchAllName = "";
    
    public abstract void Init();
    public abstract void LoadDefaultPreset();
    
    public void LoadCategories(string categoriesJsonFile)
    {
        using (StreamReader r = new StreamReader(categoriesJsonFile))
        {
            string json = r.ReadToEnd();
            var customCategoryTranslationsString = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            
            PlainNameToCodeNames = customCategoryTranslationsString;
            CustomCategories = customCategoryTranslationsString.Keys.ToList();
        }
        
        PlainNameToCodeNames[CatchAllName] = AllObjects.Select(i => i.CodeName).ToList();
        PlainNamesList = [CatchAllName];
        
        PlainNamesList.AddRange(CustomCategories);
        
        PlainNamesList.AddRange(AllObjects.Select(i => i.CustomName).Order());
        
        foreach (var objectData in AllObjects)
        {
            PlainNameToCodeNames[objectData.CustomName] = [objectData.CodeName];
        }
    }
    
    public void ApplyOopsAll(string objectCodeName)
    {
        CustomPlacementRules = new Dictionary<string, Dictionary<string, float>>()
        {
            {CatchAllName, new Dictionary<string, float>() {{objectCodeName, 1}}}
        };
        FrequencyAdjustments.Clear();
        Excluded.Clear();
        ExcludedCodeNames.Clear();
                
        NotRandomized.Clear();
        NotRandomizedCodeNames.Clear();
    }
    
    public void LoadFromJson(string pathToJson)
    {
        using (StreamReader r = new StreamReader(pathToJson))
        {
            string json = r.ReadToEnd();
            var presetData = JsonConvert.DeserializeObject<CustomPlacementPreset>(json);
            NotRandomized.Clear();
            NotRandomizedCodeNames.Clear();
            Excluded.Clear();
            ExcludedCodeNames.Clear();
            
            foreach (var notRandomized in presetData.NotRandomized)
            {
                AddNotRandomized(notRandomized);
            }
            foreach (var excluded in presetData.Excluded)
            {
                AddExcluded(excluded);
            }
            CustomPlacementRules = presetData.CustomPlacement;
            FrequencyAdjustments = presetData.FrequencyAdjustments;
        }
    }
    
    public void SaveToJson(string pathToJson)
    {
        using StreamWriter r = new StreamWriter(pathToJson);
        var presetData = new CustomPlacementPreset(NotRandomized, Excluded, CustomPlacementRules, FrequencyAdjustments);
        string json = JsonConvert.SerializeObject(presetData, Formatting.Indented);
        r.Write(json);
    }

    public void AddExcluded(string plainName)
    {
        Excluded.Add(plainName);
        ExcludedCodeNames.AddRange(PlainNameToCodeNames[plainName]);
    }

    public void RemoveExcluded(string plainName)
    {
        Excluded.Remove(plainName);
        ExcludedCodeNames = ExcludedCodeNames.Except(PlainNameToCodeNames[plainName]).ToList();
    }
    
    public void AddNotRandomized(string plainName)
    {
        NotRandomized.Add(plainName);
        NotRandomizedCodeNames.AddRange(PlainNameToCodeNames[plainName]);
    }

    public void RemoveNotRandomized(string plainName)
    {
        NotRandomized.Remove(plainName);
        NotRandomizedCodeNames = NotRandomizedCodeNames.Except(PlainNameToCodeNames[plainName]).ToList();
    }
    
    public void SetCustomPlacement(string from, string to, float frequency)
    {
        if (!CustomPlacementRules.ContainsKey(from))
        {
            CustomPlacementRules[from] = new Dictionary<string, float>();
        }

        CustomPlacementRules[from][to] = frequency;
    }

    public void RemoveCustomPlacement(string from, string to)
    {
        if (!CustomPlacementRules.ContainsKey(from) || !CustomPlacementRules[from].ContainsKey(to))
        {
            return;
        }

        CustomPlacementRules[from].Remove(to);
    }

    public List<string> PlainNamesToCodeNames(IEnumerable<string> plainNames)
    {
        var result = new List<string>();
        foreach (var plainName in plainNames)
        {
            result.AddRange(PlainNameToCodeNames[plainName]);
        }
        return result;
    }
    
    public Dictionary<string, float> CustomCategoryDictionaryToCodeNames(Dictionary<string, float> from, bool adjustForCategorySize=false)
    {
        Dictionary<string, float> result = new Dictionary<string, float>();
        foreach (var pair in from)
        {
            var translatedKey = PlainNameToCodeNames[pair.Key];
            foreach (var codeName in translatedKey)
            {
                result[codeName] = pair.Value;
                if (adjustForCategorySize)
                {
                    result[codeName] /= translatedKey.Count;
                }
            }
        }

        return result;
    }

    public void ResetRules()
    {
        Excluded.Clear();
        ExcludedCodeNames.Clear();
        NotRandomized.Clear();
        NotRandomizedCodeNames.Clear();
        CustomPlacementRules = new Dictionary<string, Dictionary<string, float>>();
        FrequencyAdjustments = new Dictionary<string, float>();
    }

    public void Update()
    {
        FinalReplacementFrequencies.Clear();
        var orderedCustomPlacementKeys = CustomPlacementRules.Keys.OrderBy(k => CategoryOrder.IndexOf(k));
        var translatedFrequencyAdjustments = CustomCategoryDictionaryToCodeNames(FrequencyAdjustments);
        foreach (var customPlacementKey in orderedCustomPlacementKeys)
        {
            var placementCodeNames = PlainNameToCodeNames[customPlacementKey];
            foreach (var codeName in placementCodeNames)
            {
                if (FinalReplacementFrequencies.ContainsKey(codeName))
                {
                    continue;
                }
                var unadjustedFrequencies = CustomCategoryDictionaryToCodeNames(CustomPlacementRules[customPlacementKey], true);
                foreach (var frequency in unadjustedFrequencies)
                {
                    if (translatedFrequencyAdjustments.ContainsKey(frequency.Key))
                    {
                        unadjustedFrequencies[frequency.Key] *= translatedFrequencyAdjustments[frequency.Key];
                    }
                }

                if (unadjustedFrequencies.Any())
                {
                    FinalReplacementFrequencies[codeName] = unadjustedFrequencies;
                }
            }
        }
        UpdateDefaultFrequencies(translatedFrequencyAdjustments);
    }

    public void UpdateDefaultFrequencies(Dictionary<string, float> translatedFrequencyAdjustments)
    {
        DefaultFrequencies = AllObjects.Select(e => new KeyValuePair<string,float>(e.CodeName, translatedFrequencyAdjustments.ContainsKey(e.CodeName) ?  translatedFrequencyAdjustments[e.CodeName] : 1)).ToDictionary();
        DefaultFrequencies = DefaultFrequencies.Where(kv => kv.Value > 0.0001).ToDictionary();
    }

    public string GetTrulyRandom()
    {
        return Utils.GetRandomWeighted(DefaultFrequencies, ExcludedCodeNames);
    }

    public string GetCategory(string codeName)
    {
        foreach (var setCategory in CustomPlacementRules.Keys)
        {
            if (PlainNameToCodeNames[setCategory].Contains(codeName))
            {
                return setCategory;
            }
        }

        return codeName;
    }

    public List<string> GetPossibleReplacements(string codeName, bool allowExcluded=true)
    {
        var replacementCategory = GetCategory(codeName);
        if (CustomPlacementRules.ContainsKey(codeName))
        {
            var replacements = CustomPlacementRules[replacementCategory];
            var plainReplacementNames = replacements.Keys.Where(k => replacements[k] > 0.0001).ToList();
            if (allowExcluded)
                return PlainNamesToCodeNames(plainReplacementNames);
            return PlainNamesToCodeNames(plainReplacementNames.Where(n => !Excluded.Contains(n)));
        }
        var result = DefaultFrequencies.Where(kv => kv.Value > 0.0001).Select(kv => kv.Key).ToList();
        return allowExcluded ? result : result.Where(n => !ExcludedCodeNames.Contains(n)).ToList();
    }
    
    public string Replace(string originalCodeName)
    {
        if (NotRandomizedCodeNames.Contains(originalCodeName))
        {
            return originalCodeName;
        }

        if (!FinalReplacementFrequencies.TryGetValue(originalCodeName, out var frequency))
            return GetTrulyRandom();
        
        var newItem = Utils.GetRandomWeighted(
            frequency,
            ExcludedCodeNames
        );
        
        return newItem != null ? newItem : originalCodeName;
    }
}