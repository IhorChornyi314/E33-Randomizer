namespace E33Randomizer;

public partial class RandomizeLocationsTab : TabBase
{

    public RandomizeLocationsTab()
    {
        InitializeComponent();
    }
}

public static class DesignLocationSettingsViewModel
{
    public static SettingsViewModel SettingsViewModel => new()
    {
        RandomizeLocations = true,
        RescaleLocations = true,
        ScaleModifierPercentage = 50,
        ScaleOptionalAreas = true,
        RescaleCharacters = true,
        RandomizeStartingLocation = true,
        RandomizeManorDoors = true,
        RandomizeWorkshopEntries = true,
        RandomizeCutsceneTeleports = true,
        RandomizeGestralBeachPortals = true
    };

}