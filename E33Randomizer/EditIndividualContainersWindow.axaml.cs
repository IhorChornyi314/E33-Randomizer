using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using E33Randomizer.ObjectDatum;
using E33Randomizer.RadomizationLogic;
using UAssetAPI.Unversioned;

namespace E33Randomizer;

public partial class EditIndividualContainersWindow : Window
{
    public EditIndividualObjectsWindowViewModel ViewModel { get; set; }
    public BaseController Controller { get; set; }
    private TextBox? _addComboTextBox;

    // Used by Design Time
    public EditIndividualContainersWindow()
    {
        RandomizerLogic.mappings = new Usmap($"{RandomizerLogic.DataDirectory}/Mappings.usmap");
        Owner = null!;
        Controller = new EnemiesController();
        Controller.Initialize();
        InitializeComponent();
        SetupAutoCompleteBehaviors();
        ViewModel = Controller.ViewModel;
        DataContext = ViewModel;
    }

    public EditIndividualContainersWindow(BaseController controller, Window owner)
    {
        Owner = owner;
        Controller = controller;
        InitializeComponent();
        SetupAutoCompleteBehaviors();
        ViewModel = controller.ViewModel;
        DataContext = ViewModel;
    }

    private void SetupAutoCompleteBehaviors()
    {
        AddObjectComboBox.GotFocus += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                AddObjectComboBox.IsDropDownOpen = true;
            });
        };
    }

    
    private void CategoryTreeView_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is ContainerViewModel selectedContainer)
        {
            ViewModel.OnContainerSelected(selectedContainer);
        }
    }

    private void AddObjectButton_Click(object sender, RoutedEventArgs e)
    {
        var container = ViewModel.CurrentContainer;
        if (container == null) return;

        var selected = ViewModel.SelectedAddObject;
        if (selected == null) return;

        Controller.AddObjectToContainer(selected.CodeName, container.CodeName);

        ViewModel.SelectedAddObject = null;
        ViewModel.AddObjectFilterTerm = string.Empty;
    }


    private void RemoveObject_Click(object sender, RoutedEventArgs e)
    {
        var container = ViewModel.CurrentContainer;
        if (container == null) return;

        if (sender is not Button button) return;
        if (button.Tag is not ObjectViewModel selectedObject) return;

        // Use the same list the UI is showing
        var idx = ViewModel.DisplayedObjects.IndexOf(selectedObject);
        if (idx < 0) return;

        Controller.RemoveObjectFromContainer(idx, container.CodeName);
    }


    public async void RegenerateData(object sender, RoutedEventArgs e)
    {
        try
        {
            Controller.Randomize();
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, $"Error generating: {ex.Message}", 
                "Reroll Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
            await File.WriteAllTextAsync("crash_log.txt", ex.ToString(), Encoding.UTF8);
        }
    }
        
    public async void PackCurrentData(object sender, RoutedEventArgs e)
    {
        try
        {
            RandomizerLogic.usedSeed = RandomizerLogic.Settings.Seed != -1 ? RandomizerLogic.Settings.Seed : Environment.TickCount; 
            RandomizerLogic.PackAndConvertData();
            await MessageDialog.ShowAsync(this, $"Generation done! You can find the mod in the rand_{RandomizerLogic.usedSeed} folder.\n\n" +
                                                $"Used Seed: {RandomizerLogic.usedSeed}\n",
                "Generation Summary", nameof(DialogBoxButton.OK), MessageBoxIcons.Information);
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, $"Error packing: {ex.Message}", 
                "Packing Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
            await File.WriteAllTextAsync("crash_log.txt", ex.ToString(), Encoding.UTF8);
        }
    }

    public async void ReadDataFromTxt(object sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = GetTopLevel(this);
            if (topLevel is null) return;

            var storage = topLevel.StorageProvider;

            var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Load TXT",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("TXT files (*.txt)") { Patterns = ["*.txt"] },
                    new FilePickerFileType("All Files") { Patterns = ["*"] }
                ]
            });

            if (files.Count != 1) return;
            
            try
            {
                Controller.ReadTxt(files[0].Path.LocalPath);
            }
            catch (Exception ex)
            {
                await MessageDialog.ShowAsync(this, $"Error loading TXT: {ex.Message}", 
                    "Load Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
                await File.WriteAllTextAsync("crash_log.txt", ex.ToString(), Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, $"Error Loading txt", "Error", nameof(DialogBoxButton.OK),  MessageBoxIcons.Error);
            await File.WriteAllTextAsync("crash_log.txt", ex.ToString(), Encoding.UTF8);
        }
    }

    public async void SaveDataAsTxt(object sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = GetTopLevel(this);
            if (topLevel is null) return;

            var storage = topLevel.StorageProvider;

            var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = "Save TXT",
                DefaultExtension = ".txt",
                FileTypeChoices = 
                [
                    new FilePickerFileType("TXT files (*.txt)") { Patterns = ["*.txt"] },
                    new FilePickerFileType("All Files") { Patterns = ["*"] }
                ]
            });

            if (file is not null)
            {
                try
                {
                    Controller.WriteTxt(file.Path.LocalPath);
                    await MessageDialog.ShowAsync(this, "TXT saved successfully!", 
                        "Save Complete", nameof(DialogBoxButton.OK), MessageBoxIcons.Information);
                }
                catch (Exception ex)
                {
                    await MessageDialog.ShowAsync(this, $"Error saving TXT: {ex.Message}", 
                        "Save Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
                    await File.WriteAllTextAsync("crash_log.txt", ex.ToString(), Encoding.UTF8);
                }
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, $"Error Saving txt", "Error", nameof(DialogBoxButton.OK),  MessageBoxIcons.Error);
            await File.WriteAllTextAsync("crash_log.txt", ex.ToString(), Encoding.UTF8);
        }
    }
}

public partial class EditIndividualObjectsWindowViewModel : ObservableObject
{
    public const string TitleResxName =  "Title";
    public const string ContainersResxName =  "Containers";
    public const string AddObjectResxName =  "AddObject";
    public const string ObjectsResxName =  "Objects";
    public const string LoadTxtResxName =  "LoadTxt";
    public const string SaveTxtResxName =  "SaveTxt";
    public const string SearchResxName =  "Search";
    
    // --- Add-object filtering ---

    public string AddObjectFilterTerm
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
            RefreshAddObjectsFilter();
        }
    } = string.Empty;

    [ObservableProperty]
    public partial ObjectViewModel? SelectedAddObject { get; set; }

    public ObservableCollection<ObjectViewModel> AllObjects
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;
            field = value;
            OnPropertyChanged();

            // Rebuild the independent view when the collection instance changes
            AddObjectsView = new DataGridCollectionView(field);
            AddObjectsView.Filter = AddObjectsFilter;
            OnPropertyChanged(nameof(AddObjectsView));

            RefreshAddObjectsFilter();
        }
    } = [];

    // This is what the Add ComboBox binds to
    public IDataGridCollectionView AddObjectsView { get; private set; }

    private bool AddObjectsFilter(object obj)
    {
        if (obj is not ObjectViewModel o) return false;

        if (string.IsNullOrWhiteSpace(AddObjectFilterTerm))
            return true;

        var term = AddObjectFilterTerm.Trim().ToLowerInvariant();
        return o.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase)
               || o.CodeName.Contains(term, StringComparison.InvariantCultureIgnoreCase);
    }

    private void RefreshAddObjectsFilter()
    {
        AddObjectsView.Refresh();
    }

    // --- Existing stuff you already had ---

    public string ContainerName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;

    public List<CategoryViewModel> Categories { get; set; } = [];
    public ObservableCollection<CategoryViewModel> FilteredCategories { get; set; } = [];
    public ObservableCollection<ObjectViewModel> DisplayedObjects { get; set; } = [];

    public ContainerViewModel? CurrentContainer;

    [ObservableProperty] 
    public partial string SearchTerm { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool CanAddObjects { get; set; } 

    public EditIndividualObjectsWindowViewModel()
    {
        // Create a default view over the default AllObjects instance
        AddObjectsView = new DataGridCollectionView(AllObjects);
        AddObjectsView.Filter = AddObjectsFilter;
    }
    
    public void UpdateFilteredCategories() => OnSearchTermChanged(string.Empty);

    partial void OnSearchTermChanged(string value)
    {
        FilteredCategories.Clear();

        var term = SearchTerm.ToLowerInvariant();

        foreach (var category in Categories)
        {
            var newCategory = new CategoryViewModel
            {
                CategoryName = category.CategoryName,
                Containers = new AvaloniaList<ContainerViewModel>(
                    category.Containers
                        .OrderBy(c => c.Name)
                        .Where(c =>
                            c.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase) ||
                            c.CodeName.Contains(term, StringComparison.InvariantCultureIgnoreCase) ||
                            c.Objects.Any(o =>
                                o.CodeName.Contains(term, StringComparison.InvariantCultureIgnoreCase) ||
                                o.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase)
                            )
                        )
                )
            };

            if (newCategory.Containers.Count > 0)
                FilteredCategories.Add(newCategory);
        }
    }

    public void UpdateDisplayedObjects()
    {
        DisplayedObjects.Clear();

        if (CurrentContainer == null) return;

        foreach (var objectViewModel in CurrentContainer.Objects)
        {
            objectViewModel.InitComboBox(AllObjects);
            DisplayedObjects.Add(objectViewModel);
        }
    }

    public void OnContainerSelected(ContainerViewModel container)
    {
        CurrentContainer = container;
        CanAddObjects = container.CanAddObjects;
        UpdateDisplayedObjects();
    }
}


public partial class CategoryViewModel :ObservableObject
{
    
    [ObservableProperty]
    public partial string CategoryName { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial AvaloniaList<ContainerViewModel> Containers { get; set; } = [];
}

public partial class ContainerViewModel : ObservableObject
{
    public ContainerViewModel(string containerCodeName, string? containerCustomName = null)
    {
        CodeName = containerCodeName;
        Name = string.IsNullOrWhiteSpace(containerCustomName) ? containerCodeName : containerCustomName;
    }

    public bool CanAddObjects { get; set; } = true;
    public string CodeName { get; set; }
    public string Name { get; set; }

    [ObservableProperty] 
    public partial AvaloniaList<ObjectViewModel> Objects { get; set; } = [];
}

public partial class ObjectViewModel : ObservableObject
{
    private int _lastIntPropertyValue = 1;
    public string Name { get; set; }
    
    [ObservableProperty]
    public partial AvaloniaList<ObjectViewModel> AllObjects { get; set; } = new();
    
    public bool CanDelete { get; set; } = true;

    public bool HasIntPropertyControl => IntProperty != -1;

    [NotifyPropertyChangedFor(nameof(HasIntPropertyControl))]
    [ObservableProperty]
    public partial int IntProperty { get; set; } = -1;

    public bool HasBoolPropertyControl { get; set; }
    public bool BoolProperty { get; set; }

    public ObjectViewModel? SelectedComboBoxValue
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            if (field != null)
            {
                Name = field.Name;
                CodeName = field.CodeName;
                if (HasIntPropertyControl)
                {
                    _lastIntPropertyValue = IntProperty;
                }

                IntProperty = !field.HasIntPropertyControl ? -1 : _lastIntPropertyValue;
            }
        }
    }

    public ObjectViewModel(ObjectData objectData)
    {
        CodeName = objectData.CodeName;
        Name = objectData.CustomName;
    }

    public void InitComboBox(ObservableCollection<ObjectViewModel> allObjects)
    {
        AllObjects.Clear();
        foreach (var o in allObjects)
        {
            AllObjects.Add(o);
        }
        SelectedComboBoxValue = AllObjects.FirstOrDefault(o => o.CodeName == CodeName);
    }
        
    public int Index { get; set; }
    public string CodeName { get; private set; }
  
    public override string ToString()
    {
        return Name;
    }
}

public class InvidiualContainersResourceLookupConverter : ResourceLookupConverter
{
    protected override string Prefix => "IndividualContainers_";
}