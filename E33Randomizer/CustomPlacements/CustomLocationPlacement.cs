using E33Randomizer.RandomizationLogic;

namespace E33Randomizer.CustomPlacements;

public class CustomLocationPlacement: CustomPlacementWindowViewModel
{
    public override string EntityType => "Location";
    
    public override void Init()
    {
        AllObjects = Controllers.LocationController.ObjectsData;
        CatchAllName = "Anything";
        CategoryOrder = new List<string>
        {
            "World Map Locations", "Act I World Map Locations", "Act II World Map Locations", "Act III World Map Locations",  
            "Act I Locations",  "Act II Locations",  "Act III Locations",  "Side Locations",  
            "Fixed-Camera Locations", "Non-portal Locations", "Anything"
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
            { "Act I Locations", [new KeyValuePair<string, byte>("Act I Locations", 100)] },
            { "Act II Locations", [new KeyValuePair<string, byte>("Act II Locations", 100)] },
            { "Act III Locations", [new KeyValuePair<string, byte>("Act III Locations", 100)] },
            { "Act I World Map Locations", [new KeyValuePair<string, byte>( "Act I World Map Locations", 100)] },
            { "Act II World Map Locations", [new KeyValuePair<string, byte>( "Act II World Map Locations", 100 )] },
            { "Act III World Map Locations", [new KeyValuePair<string, byte>( "Act III World Map Locations", 100 )] },
        });
        FinalReplacementFrequencies = new Dictionary<string, Dictionary<string, float>>();
    }
}