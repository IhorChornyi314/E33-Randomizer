using System.Windows;
using System.Windows.Controls;

namespace E33Randomizer;

public partial class RandomizeEnemiesTab : UserControl
{
    public MainWindow MainWindow;
    
    public RandomizeEnemiesTab()
    {
        InitializeComponent();
    }
    
    private void CustomEnemyPlacementButton_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.CustomEnemyPlacementButton_Click(sender, e);
    }

    private void EditEncountersButton_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.EditEncountersButton_Click(sender, e);
    }
}