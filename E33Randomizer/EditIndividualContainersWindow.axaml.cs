using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using E33Randomizer.ObjectDatum;
using E33Randomizer.RandomizationLogic;
using E33Randomizer.UIControls;
using UAssetAPI.Unversioned;

namespace E33Randomizer;

public partial class EditIndividualContainersWindow : Window
{
    private EditIndividualObjectsWindowViewModel ViewModel { get; }
    private BaseController Controller { get; }

    // Used by Design Time
    // If you want a different entity type (ex: Enemy, Item, Skill, Location), just swap out the controller setup below.  (note: SkillsController depends on ItemsController.)
    // TODO: Would be nice at some point to not need to do the full initialization here and instead setup some static dummy data for design time instead.  But for now, that's a bit too much overhead.
    //  It makes designtime generation slower since it has to go load all the json files, but it's better than nothing
    public EditIndividualContainersWindow()
    {
        RandomizerLogic.mappings = new Usmap($"{RandomizerLogic.DataDirectory}/Mappings.usmap");
        Owner = null!;
        // Controllers.ItemsController.Initialize();
        // Controllers.SkillsController.Initialize();
        Controllers.EnemiesController.Initialize();
        //Controllers.LocationController.Initialize();
        Controller = Controllers.EnemiesController;
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
        AddObjectComboBox.AddAutoDropDownOnFocusAndClickHandler();
    }

    
    private void CategoryTreeView_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is ContainerViewModel selectedContainer)
        {
            ViewModel.OnContainerSelected(selectedContainer);
        }
    }

    private void AddObjectComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var container = ViewModel.CurrentContainer;
        if (container == null) return;

        if (e.AddedItems.Count == 0 || e.AddedItems[0] is not ObjectViewModel selected) return;

        Controller.AddObjectToContainer(selected.CodeName, container.CodeName);
        Dispatcher.UIThread.Post(() =>
        {
            if (SelectedEnemiesListBox.GetLogicalChildren().Last() is ListBoxItem i)
            {
                i.Focus();  // Switch to something else for focus so that the autocomplete window doesn't immediately pop back up
            }
        });
            
        Dispatcher.UIThread.Post(() =>
        {
            AddObjectComboBox.Text = string.Empty;
        });
        
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
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.IndividualContainers_ErrorGenerating),ex.Message), 
                ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_RerollError)), MessageBoxButtons.Ok, MessageBoxIcons.Error);
            await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
        }
    }
        
    public async void PackCurrentData(object sender, RoutedEventArgs e)
    {
        try
        {
            RandomizerLogic.usedSeed = RandomizerLogic.Settings.Seed != -1 ? RandomizerLogic.Settings.Seed : Environment.TickCount; 
            RandomizerLogic.PackAndConvertData();
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.IndividualContainers_GenerationDone),RandomizerLogic.usedSeed),
                ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_GenerationSummary)), MessageBoxButtons.Ok, MessageBoxIcons.Information);
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.IndividualContainers_ErrorPacking),ex.Message), 
                ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_PackingError)), MessageBoxButtons.Ok, MessageBoxIcons.Error);
            await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
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
                Title = ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_LoadTXT)),
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_TXTFilesTxt))) { Patterns = ["*.txt"] },
                    new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_AllFiles))) { Patterns = ["*"] }
                ]
            });

            if (files.Count != 1) return;
            
            try
            {
                Controller.ReadTxt(files[0].Path.LocalPath);
            }
            catch (Exception ex)
            {
                await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.IndividualContainers_ErrorLoadingTXT),ex.Message), 
                    ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_LoadError)), MessageBoxButtons.Ok, MessageBoxIcons.Error);
                await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_ErrorLoadingTxt2)), 
                ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_Error)), MessageBoxButtons.Ok,  MessageBoxIcons.Error);
            await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
        }
    }

    public async void SaveDataAsTxt(object sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = GetTopLevel(this);
            if (topLevel is null) return;

            var storage = topLevel.StorageProvider;

            var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_SaveTXT)),
                DefaultExtension = ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_Txt2)),
                FileTypeChoices = 
                [
                    new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_TXTFilesTxt))) { Patterns = ["*.txt"] },
                    new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_AllFiles))) { Patterns = ["*"] }
                ]
            });

            if (file is not null)
            {
                try
                {
                    Controller.WriteTxt(file.Path.LocalPath);
                    await MessageDialog.ShowAsync(this, ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_TXTSavedSuccessfully)), 
                        ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_SaveComplete)), MessageBoxButtons.Ok, MessageBoxIcons.Information);
                }
                catch (Exception ex)
                {
                    await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.IndividualContainers_ErrorSavingTXT),ex.Message), 
                        ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_SaveError)), MessageBoxButtons.Ok, MessageBoxIcons.Error);
                    await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
                }
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_ErrorSavingTxt2)), 
                ResourceHelper.GetString(nameof(Assets.Resources.IndividualContainers_Error)), MessageBoxButtons.Ok,  MessageBoxIcons.Error);
            await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
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

    [ObservableProperty]
    public partial ObservableCollection<ObjectViewModel> AllObjects { get; set; } = [];

    public Func<string?, object?, bool> FilterAddObjects { get; } = (search, item) =>
    {
        if (item is not ObjectViewModel o) return false;

        if (string.IsNullOrWhiteSpace(search))
            return true;

        var term = search.Trim().ToLowerInvariant();
        return o.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase)
               || o.CodeName.Contains(term, StringComparison.InvariantCultureIgnoreCase);
    };

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
    
    public void UpdateFilteredCategories() => OnSearchTermChanged(string.Empty);

    // ReSharper disable once UnusedParameterInPartialMethod
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


public partial class CategoryViewModel : ObservableObject
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

public class IndividualContainersResourceLookupConverter : ResourceLookupConverter
{
    protected override string Prefix => "IndividualContainers_";
}