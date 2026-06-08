using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using Avalonia.Collections;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using E33Randomizer.ObjectDatum;
using E33Randomizer.RandomizationLogic;
using E33Randomizer.UIControls;

namespace E33Randomizer.CustomPlacements;

public abstract partial class CustomPlacementWindowViewModel : ObservableObject
{
    public const string CustomPlacementResxName = "CustomPlacement";
    public const string FrequencyAdjustmentResxName = "FrequencyAdjustment";
    public const string NotRandomizedResxName =  "NotRandomized";
    public const string ExcludedResxName =  "Excluded";
    public const string TitleResxName =  "Title";
    
    
    public abstract string EntityType { get; }
    
    public ObservableCollection<string> NotRandomizedOptions { get; } = [];
    public ObservableCollection<string> NotRandomized { get; } = [];
    
    public ObservableCollection<string> ExcludedOptions { get; } = [];
    public ObservableCollection<string> Excluded { get; } = [];

    [ObservableProperty]
    private string _customPlacementSearch = "";
    
    public ObservableCollection<string> CustomPlacementOptions { get; } = [];
    public AvaloniaList<CustomPlacementOption> FilteredCustomPlacementOptions { get; } = [];
    
    [ObservableProperty]
    public partial bool ShowOnlyOverridenCategories { get; set; }
    
    public List<string> NotRandomizedCodeNames = [];
    public List<string> ExcludedCodeNames = [];

    public bool JsonSyntaxHighlighting { get; set; } = true;
    public ObservableCollection<string> OopsAllObjects { get; set; } = [];
    
    public Dictionary<string, List<string>> PlainNameToCodeNames = new();
    
    [ObservableProperty]
    public partial ObservableCollection<string> CustomCategories { get; set; } = []; 
    
    [ObservableProperty]
    public partial ObservableCollectionWithChildListener<StringDictionaryKeyValuePairViewModel<StringByteKeyValuePairViewModel>> CustomPlacementRules { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollectionWithChildListener<StringByteKeyValuePairViewModel>? SelectedCustomPlacementRule { get; set; } = [];
    
    [ObservableProperty]
    public partial string SelectedCustomPlacementRuleName { get; set; }
    
    [ObservableProperty]
    public partial ObservableCollectionWithChildListener<StringByteKeyValuePairViewModel> FrequencyAdjustments { get; set; } = [];
    
    [ObservableProperty] 
    public partial bool SelectedPresetIsOops { get; set; } = false;

    [ObservableProperty]
    public partial ObservableCollectionWithChildListener<MenuItemViewModel> PresetFiles { get; set; } = [];
    
    [ObservableProperty] 
    public partial string Json { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string? OopsAllObjectsSelection { get; set; } = null;
    
    public Dictionary<string, float> DefaultFrequencies = new();
    public Dictionary<string, Dictionary<string, float>> FinalReplacementFrequencies = new();
    public List<string> CategoryOrder = new();
    public IEnumerable<ObjectData> AllObjects = [];
    
    protected string CatchAllName = "";
    
    public abstract void Init();
    public abstract void LoadDefaultPreset();
    public string[] CategoryDisplayOrder { get; set; } = [];
    public string[] AdditionalCategoriesToAdd { get; set; } = [];
    
    public virtual Func<string[], string, int> CategorySorter { get; } = (sortList, s) =>
        sortList.IndexOf(s) >= 0 ? sortList.IndexOf(s) : int.MaxValue;

    protected CustomPlacementWindowViewModel()
    {
        // Not sure how these could be null, but the compiler is convinced it's possible, so we're protecting against it.
        FrequencyAdjustments ??= [];
        CustomPlacementRules ??= [];
        PresetFiles ??= [];
        
        FrequencyAdjustments.CollectionChanged += (_, _) => UpdateJsonTextBox();
        NotRandomized.CollectionChanged += (_, _) => UpdateJsonTextBox();
        Excluded.CollectionChanged += (_, _) => UpdateJsonTextBox();
        CustomPlacementRules.CollectionChanged += (_, _) => UpdateJsonTextBox();
        FrequencyAdjustments.ItemPropertyChanged += (_, _) => UpdateJsonTextBox();
        CustomPlacementRules.ItemPropertyChanged  += OnCustomPlacementRulesOnItemPropertyChanged;
        Init();
        ApplyCustomPlacementOptionFilter();
        
        PresetFiles.Add("Oops All...", "OopsAll");
    }

    private void OnCustomPlacementRulesOnItemPropertyChanged(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
    {
        var temp = FilteredCustomPlacementOptions.SingleOrDefault(x => x.Option == SelectedCustomPlacementRuleName);
        if (temp != null)
        {
            var hasRules = CustomPlacementRules.Any(y => y.Key == SelectedCustomPlacementRuleName && y.Value.Count > 0);
            temp.HasRules = hasRules;
        }

        UpdateJsonTextBox();
    }

    partial void OnCustomPlacementSearchChanged(string value)
    {
        ApplyCustomPlacementOptionFilter();
    }

    partial void OnShowOnlyOverridenCategoriesChanged(bool value)
    {
        ApplyCustomPlacementOptionFilter();
    }

    private void ApplyCustomPlacementOptionFilter()
    {
        FilteredCustomPlacementOptions.Clear();
        var filtered = string.IsNullOrEmpty(CustomPlacementSearch)
            ? CustomPlacementOptions
            : CustomPlacementOptions.Where(o =>
                o.Contains(CustomPlacementSearch, StringComparison.InvariantCultureIgnoreCase));
        
        FilteredCustomPlacementOptions.AddRange(filtered
            .Select(x => new CustomPlacementOption(x,CustomPlacementRules.Any(y => y.Key == x && y.Value.Count > 0)))
            .Where(x => !ShowOnlyOverridenCategories || x.HasRules));
    }
    
    [RelayCommand]
    private void AddFrequencyRow()
    {
        FrequencyAdjustments.Add("", 0);
    }

    [RelayCommand]
    private void RemoveFrequencyRow(StringByteKeyValuePairViewModel model)
    {
        if (string.IsNullOrEmpty(model.Key)) return;
        
        FrequencyAdjustments.Remove(model);
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
            CustomCategories.Clear();
            CustomCategories.AddRange(customCategoryTranslationsString.Keys.ToList());
        }
        
        PlainNameToCodeNames[CatchAllName] = AllObjects.Select(i => i.CodeName).ToList();

        AddToLists(NotRandomizedOptions);
        AddToLists(ExcludedOptions);
        AddToLists(OopsAllObjects);
        AddToLists(CustomPlacementOptions);
        
        foreach (var objectData in AllObjects)
        {
            PlainNameToCodeNames[objectData.CustomName] = [objectData.CodeName];
        }

        return;

        void AddToLists(ObservableCollection<string> collection)
        {
            var temp = new List<string> { CatchAllName };
            temp.AddRange(CustomCategories);
            temp.AddRange(AllObjects.Select(i => i.CustomName));
            temp.AddRange(AdditionalCategoriesToAdd);

            collection.Clear();
            collection.AddRange(temp.OrderBy(s => CategorySorter(CategoryDisplayOrder, s)).ThenBy(s => s));
        }
    }

    protected void AddToAllSelectionLists(IEnumerable<string> value)
    {
        var toAdd = value as string[] ?? value.ToArray();
        NotRandomizedOptions.AddRange(toAdd);
        ExcludedOptions.AddRange(toAdd);
        OopsAllObjects.AddRange(toAdd);
        CustomPlacementOptions.AddRange(toAdd);

    }

    partial void OnOopsAllObjectsSelectionChanged(string? value)
    {
        ApplyOopsAll();
    }

    public void ApplyOopsAll()
    {
        if (OopsAllObjectsSelection == null) return;
        
        CustomPlacementRules.Clear();
        CustomPlacementRules.Add(CatchAllName, new List<KeyValuePair<string, byte>> { new(OopsAllObjectsSelection, 100) });
        
        FrequencyAdjustments.Clear();
        Excluded.Clear();
        ExcludedCodeNames = [];

        NotRandomized.Clear();
        NotRandomizedCodeNames.Clear();
        SelectedCustomPlacementRule = null;
        ApplyCustomPlacementOptionFilter();
    }

    public void LoadFromPreset(CustomPlacementPreset preset)
    {
        NotRandomized.Clear();
        NotRandomizedCodeNames.Clear();
        Excluded.Clear();
        ExcludedCodeNames = [];
            
        foreach (var notRandomized in preset.NotRandomized)
        {
            AddNotRandomized(notRandomized);
        }
        foreach (var excluded in preset.Excluded)
        {
            AddExcluded(excluded);
        }
        CustomPlacementRules.Clear();
        CustomPlacementRules.AddRange(preset.CustomPlacement.Select(kvp => new KeyValuePair<string, IEnumerable<KeyValuePair<string,byte>>>(kvp.Key, kvp.Value)));
        
        FrequencyAdjustments.Clear();
        FrequencyAdjustments.AddRange(preset.FrequencyAdjustments);
        SelectedCustomPlacementRule = null;
        ApplyCustomPlacementOptionFilter();
    }
    
    [RelayCommand]
    public void LoadFromJson(string pathToJson)
    {
        
        if (pathToJson == "OopsAll")
        {
            SelectedPresetIsOops = true;

            // Effectively reset everything to "nothing configured"
            LoadFromPreset(new CustomPlacementPreset([], [], new Dictionary<string, Dictionary<string, byte>>(), new Dictionary<string, byte>()));
        }
        else
        {
            SelectedPresetIsOops = false;
            using var r = new StreamReader(pathToJson.Replace("Data", RandomizerLogic.DataDirectory));
        
            var json = r.ReadToEnd();
            var presetData = JsonSerializer.DeserializeThrowOnNull(json, JsonSourceGenerationContext.Default.CustomPlacementPreset);
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

    [RelayCommand]
    public void RemoveExcluded(string plainName)
    {
        Excluded.Remove(plainName);
        ExcludedOptions.Add(plainName);
        ExcludedCodeNames = ExcludedCodeNames.Except(PlainNameToCodeNames[plainName]).ToList();
    }

    public void AddExcluded(string plainName)
    {
        Excluded.Add(plainName);
        ExcludedCodeNames = [..ExcludedCodeNames, ..PlainNameToCodeNames[plainName]];
    }
    
    public void AddNotRandomized(string plainName)
    {
        NotRandomized.Add(plainName);
        NotRandomizedCodeNames.AddRange(PlainNameToCodeNames[plainName]);
    }

    [RelayCommand]
    public void RemoveNotRandomized(string plainName)
    {
        NotRandomized.Remove(plainName);
        NotRandomizedOptions.Add(plainName);
        NotRandomizedCodeNames = NotRandomizedCodeNames.Except(PlainNameToCodeNames[plainName]).ToList();
    }

    public bool IsRandomized(string codeName)
    {
        // TODO: Strictly speaking we should also catch the case when a thing is rolled into itself 100% of the time but eh
        return !NotRandomizedCodeNames.Contains(codeName);
    }

    [RelayCommand]
    private void RemoveCustomPlacement(string to)
    {
        SelectedCustomPlacementRule?.Remove(to);
    }

    [RelayCommand]
    private void AddCustomPlacement(string from)
    {
        SelectedCustomPlacementRule?.Add("", 0);
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
    
    private Dictionary<string, float> CustomCategoryDictionaryToCodeNames(Dictionary<string, float> from, bool adjustForCategorySize=false)
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
    
    public Dictionary<string, float> CustomCategoryDictionaryToCodeNames(ObservableCollection<StringByteKeyValuePairViewModel> from, bool adjustForCategorySize=false)
    {
        Dictionary<string, float> result = new Dictionary<string, float>();
        foreach (var pair in from)
        {
            var translatedKey = PlainNameToCodeNames[pair.Key];
            foreach (var codeName in translatedKey)
            {
                result[codeName] = pair.Value / 100f;
                if (adjustForCategorySize)
                {
                    result[codeName] /=  translatedKey.Count;
                }
            }
        }

        return result;
    }

    public void ResetRules()
    {
        Excluded.Clear();
        ExcludedCodeNames = [];
        NotRandomized.Clear();
        NotRandomizedCodeNames.Clear();
        CustomPlacementRules.Clear();
        FrequencyAdjustments.Clear();
    }

    public void Update()
    {
        FinalReplacementFrequencies.Clear();
        var orderedCustomPlacementKeys = CustomPlacementRules.Select(x => x.Key).OrderBy(k => CategoryOrder.IndexOf(k));
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

                if (!CustomPlacementRules.TryGetValue(customPlacementKey, out var customPlacementRule))
                {
                    continue;
                }
                
                var unadjustedFrequencies = CustomCategoryDictionaryToCodeNames(customPlacementRule.ToDictionary(x => x.Key, x=> x.Value / 100f), true);
                foreach (var frequency in unadjustedFrequencies)
                {
                    if (translatedFrequencyAdjustments.TryGetValue(frequency.Key, out var adjustment))  
                    {
                        unadjustedFrequencies[frequency.Key] *= adjustment;
                    }
                }

                if (unadjustedFrequencies.Count != 0)
                {
                    FinalReplacementFrequencies[codeName] = unadjustedFrequencies.ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        UpdateDefaultFrequencies(translatedFrequencyAdjustments);
    }

    public void UpdateDefaultFrequencies(Dictionary<string, float> translatedFrequencyAdjustments)
    {
        DefaultFrequencies = AllObjects.Select(e => new KeyValuePair<string,float>(e.CodeName, translatedFrequencyAdjustments.GetValueOrDefault(e.CodeName, 0.1f))).ToDictionary();
        DefaultFrequencies = DefaultFrequencies.Where(kv => kv.Value > 0.00001).ToDictionary();
    }

    public string GetTrulyRandom()
    {
        return Utils.GetRandomWeighted(DefaultFrequencies, ExcludedCodeNames);
    }

    public string GetCategory(string codeName)
    {
        foreach (var setCategory in CustomPlacementRules.Select(x => x.Key))
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
        if (CustomPlacementRules.Any(x => x.Key == replacementCategory)
            && CustomPlacementRules.TryGetValue(replacementCategory, out var replacements))
        {
            var plainReplacementNames = replacements.Where(k => replacements.Any(x => x.Key == k.Key && x.Value > 0)).Select(x => x.Key).ToList();
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
        
        return !string.IsNullOrEmpty(newItem) ? newItem : originalCodeName;
    }


    public partial class CustomPlacementOption : ObservableObject
    {
        public CustomPlacementOption()
        {
            Option = string.Empty;
        }

        public CustomPlacementOption(string option, bool hasRules)
        {
            Option = option;
            HasRules = hasRules;
        }
        
        public string Option { get; set; }

        [ObservableProperty]
        public partial bool HasRules { get; set; }
    }
}