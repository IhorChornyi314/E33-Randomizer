using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;

namespace E33Randomizer;

public partial class RandomizeItemsTab : UserControl
{

    public RandomizeItemsTab()
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