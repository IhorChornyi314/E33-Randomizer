using System.Windows;
using System.Windows.Controls;

namespace E33Randomizer;

public partial class RandomizeLocationsTab : UserControl
{
    public RandomizeLocationsTab()
    {
        InitializeComponent();
    }
    
    private void CustomPlacementButton_Click(object sender, RoutedEventArgs e)
    {
        (Application.Current.MainWindow as MainWindow).OpenCustomPlacementButton_Click(sender, e);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        (Application.Current.MainWindow as MainWindow).OpenEditObjectsButton_Click(sender, e);
    }
}