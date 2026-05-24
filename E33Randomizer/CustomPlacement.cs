

using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace E33Randomizer;

public class CustomPlacementPreset
{
    public List<string> NotRandomized { get; set; }
    public List<string> Excluded  { get; set; }
    public Dictionary<string, Dictionary<string, byte>> CustomPlacement  { get; set; }
    public Dictionary<string, byte> FrequencyAdjustments { get; set; }

    [Obsolete("Only used for JSON Deserialization, not intended to be used directly.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CustomPlacementPreset()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        
    }
    
    public CustomPlacementPreset(List<string> n,
        List<string> e,
        Dictionary<string, Dictionary<string, byte>> c,
        Dictionary<string, byte> f)
    {
        NotRandomized = n;
        Excluded = e;
        CustomPlacement = c;
        FrequencyAdjustments = f;
    }
    
    public CustomPlacementPreset(ObservableCollection<string> n,
        ObservableCollection<string> e,
        ObservableCollectionWithChildListener<StringDictionaryKeyValuePairViewModel<StringByteKeyValuePairViewModel>> c,
        ObservableCollection<StringByteKeyValuePairViewModel> f)
    {
        NotRandomized = n.ToList();
        Excluded = e.ToList();
        CustomPlacement = c.Where(x => !x.HasErrors && !x.Value.Any(z => z.HasErrors) && x.Value.Count > 0).ToDictionary(x => x.Key, y => y.Value.ToDictionary(z => z.Key, z => z.Value));
        FrequencyAdjustments = f.Where(x => !x.HasErrors).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}

public abstract partial class CustomPlacementWindowViewModel : ObservableObject
{
    public ObservableCollection<string> NotRandomizedOptions { get; } = [];
    public ObservableCollection<string> NotRandomized { get; } = [];
    
    public ObservableCollection<string> ExcludedOptions { get; } = [];
    public ObservableCollection<string> Excluded { get; } = [];

    [ObservableProperty]
    private string _customPlacementSearch = "";
    
    public ObservableCollection<string> CustomPlacementOptions { get; } = [];
    public AvaloniaList<Tuple<string,bool>> FilteredCustomPlacementOptions { get; } = [];
    
    public List<string> NotRandomizedCodeNames = [];
    public List<string> ExcludedCodeNames = [];

    public ObservableCollection<string> OopsAllObjects { get; set; } = [];
    
    public Dictionary<string, List<string>> PlainNameToCodeNames = new();
    
    public List<string> CustomCategories { get; set; } = []; 
    
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
    
    public Dictionary<string, byte> DefaultFrequencies = new();
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
        FrequencyAdjustments.CollectionChanged += (_, _) => UpdateJsonTextBox();
        NotRandomized.CollectionChanged += (_, _) => UpdateJsonTextBox();
        Excluded.CollectionChanged += (_, _) => UpdateJsonTextBox();
        CustomPlacementRules.CollectionChanged += (_, _) => UpdateJsonTextBox();
        FrequencyAdjustments.ItemPropertyChanged += (_, _) => UpdateJsonTextBox();
        CustomPlacementRules.ItemPropertyChanged  += (_, _) =>
        {
            var temp = FilteredCustomPlacementOptions.SingleOrDefault(x => x.Item1 == SelectedCustomPlacementRuleName);
            if (temp != null)
            {
                var hasRules = CustomPlacementRules.Any(y => y.Key == SelectedCustomPlacementRuleName && y.Value.Count > 0);
                temp.Item2 = hasRules;   

            }
            
            UpdateJsonTextBox();
        };
        Init();
        ApplyCustomPlacementOptionFilter();
        
        PresetFiles.Add("Oops All...", "OopsAll");
    }

    partial void OnCustomPlacementSearchChanged(string value)
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

        
        
        FilteredCustomPlacementOptions.AddRange(filtered.Select(x => new Tuple<string, bool>(x, CustomPlacementRules.Any(y => y.Key == x && y.Value.Count > 0))));
    }
    
    [RelayCommand]
    private void AddFrequencyRow()
    {
        FrequencyAdjustments.Add("SelectOne", 0);
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
            CustomCategories = customCategoryTranslationsString.Keys.ToList();
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
    
    public void ApplyOopsAll()
    {
        if (OopsAllObjectsSelection == null) return;
        
        CustomPlacementRules.Clear();
        CustomPlacementRules.Add(CatchAllName, new List<KeyValuePair<string, byte>> { new(OopsAllObjectsSelection, 100) });
        
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
        CustomPlacementRules.Clear();
        CustomPlacementRules.AddRange(preset.CustomPlacement.Select(kvp => new KeyValuePair<string, IEnumerable<KeyValuePair<string,byte>>>(kvp.Key, kvp.Value)));
        
        FrequencyAdjustments.Clear();
        FrequencyAdjustments.AddRange(preset.FrequencyAdjustments);
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
        ExcludedCodeNames.AddRange(PlainNameToCodeNames[plainName]);
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
    
    private Dictionary<string, byte> CustomCategoryDictionaryToCodeNames(Dictionary<string, byte> from, bool adjustForCategorySize=false)
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
                    result[codeName] = (byte)(result[codeName]/ translatedKey.Count);
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
                
                var unadjustedFrequencies = CustomCategoryDictionaryToCodeNames(customPlacementRule.ToDictionary(x => x.Key, x=> x.Value), true);
                foreach (var frequency in unadjustedFrequencies)
                {
                    if (translatedFrequencyAdjustments.TryGetValue(frequency.Key, out var adjustment))  
                    {
                        unadjustedFrequencies[frequency.Key] *= adjustment;
                    }
                }

                if (unadjustedFrequencies.Count != 0)
                {
                    FinalReplacementFrequencies[codeName] = unadjustedFrequencies.ToDictionary(x => x.Key, x => x.Value / 100f);
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
            var plainReplacementNames = replacements.Where(k => replacements.Any(x => x.Key == k.Key && x.Value > 0.0001)).Select(x => x.Key).ToList();
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

    #region ExpanderControl

    [ObservableProperty]
    private int _openSection = 1;

    public bool IsSpecialRulesOpen
    {
        get => OpenSection == 0;
        set { if (value) OpenSection = 0; else if (OpenSection == 0) OpenSection = -1; }
    }

    public bool IsCustomPlacementOpen
    {
        get => OpenSection == 1;
        set { if (value) OpenSection = 1; else if (OpenSection == 1) OpenSection = -1; }
    }

    public bool IsFrequencyAdjustmentOpen
    {
        get => OpenSection == 2;
        set { if (value) OpenSection = 2; else if (OpenSection == 2) OpenSection = -1; }
    }

    partial void OnOpenSectionChanged(int value)
    {
        OnPropertyChanged(nameof(IsSpecialRulesOpen));
        OnPropertyChanged(nameof(IsCustomPlacementOpen));
        OnPropertyChanged(nameof(IsFrequencyAdjustmentOpen));
    }

    #endregion
    
}

public class CustomPlacementValueConverter : IMultiValueConverter
{
    
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        string? inputString = null;
        CustomPlacementWindowViewModel? dataContext = null;

        bool? isFound = null;
        
        foreach (var value in values)
        {
            if (value is Tuple<string, bool> tuple)
            {
                inputString = tuple.Item1;
                isFound = tuple.Item2;
            } 

            if (value is CustomPlacementWindowViewModel vm)
                dataContext = vm;
        }

        if (inputString is null && dataContext is not null)
        {
            return new Tuple<string, bool>(dataContext.SelectedCustomPlacementRuleName, isFound! ?? false);
        }
        
        if (inputString is null || dataContext is null)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (!dataContext.CustomPlacementRules.TryGetValue(inputString, out var rule))
        {
            dataContext.CustomPlacementRules.Add(inputString, []);
        }


        if (dataContext.CustomPlacementRules.TryGetValue(inputString, out rule))
        {
            dataContext.SelectedCustomPlacementRule = rule;
            dataContext.SelectedCustomPlacementRuleName = inputString;
            return new Tuple<string, bool>(inputString, isFound!.Value);
        }

        return new BindingNotification(new InvalidCastException("Attempted to create empty rule but failed"));
    }
}