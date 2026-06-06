using Avalonia.Controls;
using Avalonia.Interactivity;

namespace E33Randomizer;

public partial class RandomizeLocationsTab : UserControl
{

    public RandomizeLocationsTab()
    {
        InitializeComponent();
    }
    
    private void CustomPlacementButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().OpenCustomPlacementButton_Click(sender, e);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().OpenEditObjectsButton_Click(sender, e);
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