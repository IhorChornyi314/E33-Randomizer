using System.Windows;
using System.Windows.Controls;

namespace E33Randomizer;

public partial class RandomizeItemsTab : UserControl
{
    public MainWindow MainWindow;
    
    public RandomizeItemsTab()
    {
        InitializeComponent();
    }
    
    private void CustomItemPlacementButton_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.CustomItemPlacementButton_Click(sender, e);
    }

    private void EditChecksButton_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.EditChecksButton_Click(sender, e);
    }
}