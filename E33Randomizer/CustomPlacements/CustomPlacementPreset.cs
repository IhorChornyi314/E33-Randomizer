using System.Collections.ObjectModel;

namespace E33Randomizer.CustomPlacements;

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