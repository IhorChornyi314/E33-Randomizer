using E33Randomizer.RadomizationLogic;

namespace E33Randomizer.CustomPlacements;

public class CustomLocationPlacement: CustomPlacementWindowViewModel
{
    public override string Title => "Custom Location Placement";
    public override string EntityType => "Location";
    public override string EntityTypePlural => "Locations";

    public override void Init()
    {
        AllObjects = Controllers.LocationController.ObjectsData;
        CatchAllName = "Anything";
        CategoryOrder = new List<string>
        {
            "World Map Locations",  "Act I Locations",  "Act II Locations",  "Act III Locations",  "Side Locations",  
            "Fixed-Camera Locations",  "Anything"
        };
        
        PresetFiles = new()
        {
            {"Split categories (default)", "Data/Presets/locations/default.json"},
            {"Total randomness", "Data/Presets/locations/total_random.json"},
            {"No world map", "Data/Presets/locations/no_world_map.json"},
            {"Mostly world map", "Data/Presets/locations/world_map_only.json"},
            {"Custom preset 1", "Data/Presets/locations/custom_1.json"},
            {"Custom preset 2", "Data/Presets/locations/custom_2.json"},
        };
        
        LoadCategories($"{RandomizerLogic.DataDirectory}/location_categories.json");
        
        LoadDefaultPreset();
    }

    public override void LoadDefaultPreset()
    {
        ResetRules();
        CustomPlacementRules.AddRange(new Dictionary<string, IEnumerable<KeyValuePair<string, byte>>>
        {
            { "Act I Locations", [new KeyValuePair<string, byte>("Act I Locations", 1)] },
            { "Act II Locations", [new KeyValuePair<string, byte>("Act II Locations", 1)] },
            { "Act III Locations", [new KeyValuePair<string, byte>("Act III Locations", 1)] }
        });
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }
}