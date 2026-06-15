using Avalonia.Controls;
using Avalonia.Interactivity;

namespace E33Randomizer;

public abstract class TabBase : UserControl
{
    protected void CustomPlacementButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().OpenCustomPlacementButton_Click(sender, e);
    }

    protected void EditButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().OpenEditObjectsButton_Click(sender, e);
    }
}