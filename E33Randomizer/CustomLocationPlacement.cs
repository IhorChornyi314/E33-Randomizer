namespace E33Randomizer;

public class CustomLocationPlacement: CustomPlacement
{
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
        CustomPlacementRules = new Dictionary<string, Dictionary<string, float>>
        {
            { "Act I Locations", new Dictionary<string, float> { { "Act I Locations", 1 } } },
            { "Act II Locations", new Dictionary<string, float> { { "Act II Locations", 1 } } },
            { "Act III Locations", new Dictionary<string, float> { { "Act III Locations", 1 } } },
        };
        FrequencyAdjustments = new Dictionary<string, float>();
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }
}