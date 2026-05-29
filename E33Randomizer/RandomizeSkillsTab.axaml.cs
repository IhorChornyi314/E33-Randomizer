using Avalonia.Controls;
using Avalonia.Interactivity;

namespace E33Randomizer;

public partial class RandomizeSkillsTab : UserControl
{

    public RandomizeSkillsTab()
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

// Design Time Settings DataContext
public static class DesignSkillsSettingsViewModel
{
    public static SettingsViewModel SettingsViewModel => new()
    {
        
    };
        
}