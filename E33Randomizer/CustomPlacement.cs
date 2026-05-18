

using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace E33Randomizer;

public class CustomPlacementPreset
{
    public List<string> NotRandomized { get; set; }
    public List<string> Excluded  { get; set; }
    public Dictionary<string, Dictionary<string, float>> CustomPlacement  { get; set; }
    public Dictionary<string, byte> FrequencyAdjustments { get; set; }

    [Obsolete("Only used for JSON Deserialization, not intended to be used directly.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CustomPlacementPreset()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        
    }
    
    public CustomPlacementPreset(List<string> n,
        List<string> e,
        Dictionary<string, Dictionary<string, float>> c,
        Dictionary<string, byte> f)
    {
        NotRandomized = n;
        Excluded = e;
        CustomPlacement = c;
        FrequencyAdjustments = f;
    }
    
    public CustomPlacementPreset(List<string> n,
        List<string> e,
        Dictionary<string, Dictionary<string, float>> c,
        ObservableCollection<StringByteKeyValuePairViewModel> f)
    {
        NotRandomized = n;
        Excluded = e;
        CustomPlacement = c;
        FrequencyAdjustments = f.Where(x => !x.HasErrors).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}

public abstract partial class CustomPlacementWindowViewModel : ObservableObject
{
    public List<string> NotRandomized = [];
    public List<string> Excluded = [];
    public List<string> NotRandomizedCodeNames = [];
    public List<string> ExcludedCodeNames = [];
    
    public List<string> PlainNamesList = [];
    public Dictionary<string, List<string>> PlainNameToCodeNames = new();
    
    public List<string> CustomCategories { get; set; } = []; 
    public Dictionary<string, Dictionary<string, float>> CustomPlacementRules = new();
    
    [ObservableProperty]
    public partial ObservableCollectionWithChildListener<StringByteKeyValuePairViewModel> FrequencyAdjustments { get; set; } = [];
    
    [ObservableProperty] 
    public partial bool SelectedPresetIsOops { get; set; } = false;

    [ObservableProperty]
    public partial ObservableCollectionWithChildListener<MenuItemViewModel> PresetFiles { get; set; }
    
    [ObservableProperty] 
    public partial string Json { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string? OopsAllObjectsSelection { get; set; } = null;
    
    public Dictionary<string, byte> DefaultFrequencies = new();
    public Dictionary<string, Dictionary<string, float>> FinalReplacementFrequencies = new();
    public List<string> CategoryOrder = new();
    public IEnumerable<ObjectData> AllObjects = Array.Empty<ObjectData>();
    
    protected string CatchAllName = "";
    
    public abstract void Init();
    public abstract void LoadDefaultPreset();

    protected CustomPlacementWindowViewModel()
    {
        Init();
        FrequencyAdjustments?.CollectionChanged += (_, _) => UpdateJsonTextBox();
    }
    
    [RelayCommand]
    private void AddFrequencyRow()
    {
        FrequencyAdjustments.Add("SelectOne", 0);
    }

    [RelayCommand]
    private void OopsAllButton()
    {
        SelectedPresetIsOops = true;
    }

    [RelayCommand]
    private void RemoveFrequencyRow(StringByteKeyValuePairViewModel model)
    {
        if (string.IsNullOrEmpty(model.Key)) return;
        
        FrequencyAdjustments.Remove(model);
    }

    private bool FrequencyExists(string key)
    {
        return FrequencyAdjustments.Any(x => x.Key == key);
    }
    
    private void UpdateJsonTextBox()
    {
        Json = JsonSerializer.Serialize(new CustomPlacementPreset(NotRandomized,Excluded, CustomPlacementRules, FrequencyAdjustments), JsonSourceGenerationContextSerializationFactory.LazyJsonSourceGenerationContext.Value.CustomPlacementPreset);
    }

    
    public void LoadCategories(string categoriesJsonFile)
    {
        using (StreamReader r = new StreamReader(categoriesJsonFile))
        {
            string json = r.ReadToEnd();
            var customCategoryTranslationsString = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json, JsonSourceGenerationContext.Default.DictionaryStringListString) ?? [];
            
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

    public void LoadFromPreset(CustomPlacementPreset preset)
    {
        NotRandomized.Clear();
        NotRandomizedCodeNames.Clear();
        Excluded.Clear();
        ExcludedCodeNames.Clear();
            
        foreach (var notRandomized in preset.NotRandomized)
        {
            AddNotRandomized(notRandomized);
        }
        foreach (var excluded in preset.Excluded)
        {
            AddExcluded(excluded);
        }
        CustomPlacementRules = preset.CustomPlacement;
        FrequencyAdjustments.Clear();
        foreach (var kvp in preset.FrequencyAdjustments)
        {
            FrequencyAdjustments.Add(kvp.Key, kvp.Value);
        }
    }
    
    [RelayCommand]
    public void LoadFromJson(string pathToJson)
    {
        using (StreamReader r = new StreamReader(pathToJson.Replace("Data", RandomizerLogic.DataDirectory)))
        {
            string json = r.ReadToEnd();
            var presetData = JsonSerializer.Deserialize<CustomPlacementPreset>(json, JsonSourceGenerationContext.Default.CustomPlacementPreset);
            LoadFromPreset(presetData);
        }
    }
    
    public void SaveToJson(string pathToJson)
    {
        using StreamWriter r = new StreamWriter(pathToJson);
        var presetData = new CustomPlacementPreset(NotRandomized, Excluded, CustomPlacementRules, FrequencyAdjustments);
        string json = JsonSerializer.Serialize(presetData, JsonSourceGenerationContextSerializationFactory.LazyJsonSourceGenerationContext.Value.CustomPlacementPreset);
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

    public bool IsRandomized(string codeName)
    {
        // TODO: Strictly speaking we should also catch the case when a thing is rolled into itself 100% of the time but eh
        return !NotRandomizedCodeNames.Contains(codeName);
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
    
    public Dictionary<string, byte> CustomCategoryDictionaryToCodeNames(ObservableCollection<StringByteKeyValuePairViewModel> from, bool adjustForCategorySize=false)
    {
        Dictionary<string, byte> result = new Dictionary<string, byte>();
        foreach (var pair in from)
        {
            var translatedKey = PlainNameToCodeNames[pair.Key];
            foreach (var codeName in translatedKey)
            {
                result[codeName] = pair.Value;
                if (adjustForCategorySize)
                {
                    result[codeName] = (byte)((result[codeName] / translatedKey.Count) * 100);
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
        FrequencyAdjustments.Clear();
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

    public void UpdateDefaultFrequencies(Dictionary<string, byte> translatedFrequencyAdjustments)
    {
        DefaultFrequencies = AllObjects.Select(e => new KeyValuePair<string,byte>(e.CodeName, translatedFrequencyAdjustments.ContainsKey(e.CodeName) ?  translatedFrequencyAdjustments[e.CodeName] : (byte)1)).ToDictionary();
        DefaultFrequencies = DefaultFrequencies.Where(kv => kv.Value > 0).ToDictionary();
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
        if (NotRandomizedCodeNames.Contains(originalCodeName) || !FinalReplacementFrequencies.TryGetValue(originalCodeName, out var frequency))
        {
            return originalCodeName;
        }
        
        var newItem = Utils.GetRandomWeighted(
            frequency,
            ExcludedCodeNames
        );
        
        return newItem != null ? newItem : originalCodeName;
    }
}