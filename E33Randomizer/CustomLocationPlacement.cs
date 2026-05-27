namespace E33Randomizer;

public class CustomLocationPlacement: CustomPlacement
{
    public override void Init()
    {
        AllObjects = Controllers.LocationController.ObjectsData;
        CatchAllName = "Anything";
        CategoryOrder = new List<string>
        {
            "World Map Locations", "Act I World Map Locations", "Act II World Map Locations", "Act III World Map Locations",  
            "Act I Locations",  "Act II Locations",  "Act III Locations",  "Side Locations",  
            "Fixed-Camera Locations",  "Anything"
        };
        
        PresetFiles = new()
        {
            {"Split categories (default)", "Data/presets/locations/default.json"},
            {"Total randomness", "Data/presets/locations/total_random.json"},
            {"No world map", "Data/presets/locations/no_world_map.json"},
            {"Mostly world map", "Data/presets/locations/world_map_only.json"},
            {"Custom preset 1", "Data/presets/locations/custom_1.json"},
            {"Custom preset 2", "Data/presets/locations/custom_2.json"},
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
            { "Act I World Map Locations", new Dictionary<string, float> { { "Act I World Map Locations", 1 } } },
            { "Act II World Map Locations", new Dictionary<string, float> { { "Act II World Map Locations", 1 } } },
            { "Act III World Map Locations", new Dictionary<string, float> { { "Act III World Map Locations", 1 } } },
        };
        FrequencyAdjustments = new Dictionary<string, float>();
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }
}