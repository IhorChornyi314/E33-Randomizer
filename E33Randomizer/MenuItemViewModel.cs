using CommunityToolkit.Mvvm.ComponentModel;

namespace E33Randomizer;

public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string FilePath { get; set; }


    public MenuItemViewModel(string name, string filePath)
    {
        Name = name;
        FilePath = filePath;
    }
}