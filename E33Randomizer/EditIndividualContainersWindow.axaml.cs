using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using E33Randomizer.ObjectDatum;
using E33Randomizer.RadomizationLogic;
using E33Randomizer.UIControls;

namespace E33Randomizer
{
    public partial class EditIndividualContainersWindow : Window
    {
        public EditIndividualObjectsWindowViewModel ViewModel { get; set; }
        public BaseController Controller { get; set; }
        private ContainerViewModel? _selectedContainerViewModel = null;
        private string? _objectType = null;
        private TextBox? _addComboTextBox;

        private void AddObjectComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ComboBox cb) return;

            cb.ApplyTemplate();
            _addComboTextBox = cb.GetVisualChildren().Single(x => x.Name == "PART_EditableTextBox") as TextBox;

            if (_addComboTextBox != null)
            {
                _addComboTextBox.TextChanged -= AddComboTextBox_TextChanged;
                _addComboTextBox.TextChanged += AddComboTextBox_TextChanged;
            }
        }

        private void AddComboTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            // Only open when the user is editing this control
            if (!AddObjectComboBox.IsKeyboardFocusWithin) return;

            AddObjectComboBox.IsDropDownOpen = true;
        }

        private void AddObjectComboBox_GotKeyboardFocus(object sender, FocusChangedEventArgs e)
        {
            if (!AddObjectComboBox.IsDropDownOpen)
                AddObjectComboBox.IsDropDownOpen = true;
        }

        public EditIndividualContainersWindow(BaseController controller, Window owner)
        {
            Owner = owner;
            Controller = controller;
            InitializeComponent();
            ViewModel = controller.ViewModel;
            DataContext = ViewModel;
            ApplyObjectsType();
        }

        private void ApplyObjectsType()
        {
            var containersTextBlock = Owner?.FindNameScope()?.Find<TextBlock>("ContainersTextBlock");
            if (containersTextBlock != null) containersTextBlock.Text = ViewModel.ContainerName + "s";
            var addObjectTextBlock = Owner?.FindNameScope()?.Find<TextBlock>("AddObjectTextBlock");
            if (addObjectTextBlock != null) addObjectTextBlock.Text = $"Add {ViewModel.ObjectName.ToLower()}:";
            var objectsTextBlock = Owner?.FindNameScope()?.Find<TextBlock>("ObjectsTextBlock");
            if (objectsTextBlock != null) objectsTextBlock.Text = $"{ViewModel.ObjectName}s in the {ViewModel.ContainerName.ToLower()}".Replace("ys", "ies");
            var loadTextButton = Owner?.FindNameScope()?.Find<Button>("LoadTextButton");
            if (loadTextButton != null) loadTextButton.Content = $"Load {ViewModel.ContainerName.ToLower()}s from .txt file";
            var saveTextButton = Owner?.FindNameScope()?.Find<Button>("SaveTextButton");
            if (saveTextButton != null) saveTextButton.Content = $"Save {ViewModel.ContainerName.ToLower()}s to .txt file";
            var searchLabel = Owner?.FindNameScope()?.Find<Label>("SearchLabel");
            if (searchLabel != null) searchLabel.Content = $"Search by {ViewModel.ContainerName.ToLower()} or {ViewModel.ObjectName.ToLower()} names:";
            Title = $"Edit individual {ViewModel.ContainerName.ToLower()}s";
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


        public void RegenerateData(object sender, RoutedEventArgs e)
        {
            try
            {
                Controller.Randomize();
            }
            catch (Exception ex)
            {
                MessageDialog.Show(this, $"Error generating: {ex.Message}", 
                    "Reroll Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
                File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
            }
        }
        
        public void PackCurrentData(object sender, RoutedEventArgs e)
        {
            RandomizerLogic.usedSeed = RandomizerLogic.Settings.Seed != -1 ? RandomizerLogic.Settings.Seed : Environment.TickCount; 
            
            try
            {
                RandomizerLogic.PackAndConvertData();
                MessageDialog.Show(this, $"Generation done! You can find the mod in the rand_{RandomizerLogic.usedSeed} folder.\n\n" +
                                $"Used Seed: {RandomizerLogic.usedSeed}\n",
                    "Generation Summary", nameof(DialogBoxButton.OK), MessageBoxIcons.Information);
            }
            catch (Exception ex)
            {
                MessageDialog.Show(this, $"Error packing: {ex.Message}", 
                    "Packing Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
                File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
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
            
                if (files.Count == 1)
                {
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

        private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTextBox = Owner?.FindNameScope()?.Find<TextBox>("SearchTextBox");
            if (searchTextBox != null)
            {
                ViewModel.SearchTerm = searchTextBox.Text;
                ViewModel.UpdateFilteredCategories();
            }
        }
    }

public class EditIndividualObjectsWindowViewModel : INotifyPropertyChanged
{
    // --- Add-object filtering ---

    private string _addObjectFilterTerm = string.Empty;
    public string AddObjectFilterTerm
    {
        get => _addObjectFilterTerm;
        set
        {
            if (_addObjectFilterTerm == value) return;
            _addObjectFilterTerm = value;
            OnPropertyChanged(nameof(AddObjectFilterTerm));
            RefreshAddObjectsFilter();
        }
    }

    private ObjectViewModel? _selectedAddObject;
    public ObjectViewModel? SelectedAddObject
    {
        get => _selectedAddObject;
        set
        {
            if (_selectedAddObject == value) return;
            _selectedAddObject = value;
            OnPropertyChanged(nameof(SelectedAddObject));
        }
    }

    private ObservableCollection<ObjectViewModel> _allObjects = new();
    public ObservableCollection<ObjectViewModel> AllObjects
    {
        get => _allObjects;
        set
        {
            if (ReferenceEquals(_allObjects, value)) return;
            _allObjects = value ?? new ObservableCollection<ObjectViewModel>();
            OnPropertyChanged(nameof(AllObjects));

            // Rebuild the independent view when the collection instance changes
            AddObjectsView = new DataGridCollectionView(_allObjects);
            AddObjectsView.Filter = AddObjectsFilter;
            OnPropertyChanged(nameof(AddObjectsView));

            RefreshAddObjectsFilter();
        }
    }

    // This is what the Add ComboBox binds to
    private IDataGridCollectionView _addObjectsView;
    public IDataGridCollectionView AddObjectsView
    {
        get => _addObjectsView;
        private set => _addObjectsView = value;
    }

    private bool AddObjectsFilter(object obj)
    {
        if (obj is not ObjectViewModel o) return false;

        if (string.IsNullOrWhiteSpace(AddObjectFilterTerm))
            return true;

        var term = AddObjectFilterTerm.Trim().ToLowerInvariant();
        return (o.Name ?? "").ToLowerInvariant().Contains(term)
               || (o.CodeName ?? "").ToLowerInvariant().Contains(term);
    }

    private void RefreshAddObjectsFilter()
    {
        AddObjectsView?.Refresh();
    }

    // --- Existing stuff you already had ---

    public string ContainerName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;

    public List<CategoryViewModel> Categories { get; set; } = new();
    public ObservableCollection<CategoryViewModel> FilteredCategories { get; set; } = new();
    public ObservableCollection<ObjectViewModel> DisplayedObjects { get; set; } = new();

    public ContainerViewModel? CurrentContainer = null;

    private string _searchTerm = string.Empty;
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (_searchTerm == value) return;
            _searchTerm = value ?? string.Empty;
            OnPropertyChanged(nameof(SearchTerm));
        }
    }

    private bool _canAddObjects = true;
    public bool CanAddObjects
    {
        get => _canAddObjects;
        set
        {
            if (_canAddObjects == value) return;
            _canAddObjects = value;
            OnPropertyChanged(nameof(CanAddObjects));
        }
    }

    public EditIndividualObjectsWindowViewModel()
    {
        // Create a default view over the default AllObjects instance
        AddObjectsView = new DataGridCollectionView(AllObjects);
        AddObjectsView.Filter = AddObjectsFilter;
    }

    public void UpdateFilteredCategories()
    {
        FilteredCategories.Clear();

        var term = (SearchTerm ?? string.Empty).ToLowerInvariant();

        foreach (var category in Categories)
        {
            var newCategory = new CategoryViewModel
            {
                CategoryName = category.CategoryName,
                Containers = new ObservableCollection<ContainerViewModel>(
                    category.Containers
                        .OrderBy(c => c.Name)
                        .Where(c =>
                            (c.Name ?? "").ToLowerInvariant().Contains(term) ||
                            (c.CodeName ?? "").ToLowerInvariant().Contains(term) ||
                            c.Objects.Any(o =>
                                (o.CodeName ?? "").ToLowerInvariant().Contains(term) ||
                                (o.Name ?? "").ToLowerInvariant().Contains(term)
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

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


    public class CategoryViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public ObservableCollection<ContainerViewModel> Containers { get; set; } = new ObservableCollection<ContainerViewModel>();
    }

    public class ContainerViewModel
    {
        public ContainerViewModel(string containerCodeName, string containerCustomName="")
        {
            CodeName = containerCodeName;
            Name = containerCustomName == "" ? containerCodeName : containerCustomName;
            Objects = new ObservableCollection<ObjectViewModel>();
        }

        public bool CanAddObjects { get; set; } = true;
        public string CodeName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<ObjectViewModel> Objects { get; set; } = new ObservableCollection<ObjectViewModel>();
    }

    public class ObjectViewModel : INotifyPropertyChanged
    {
        private ObjectViewModel? _selectedComboBoxValue;
        private int _lastIntPropertyValue = 1;
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<ObjectViewModel> AllObjects { get; set; } = new ObservableCollection<ObjectViewModel>();
        public bool CanDelete { get; set; } = true;

        public bool HasIntPropertyControl => IntProperty != -1;
        private int _intProperty = -1;

        public int IntProperty
        {
            get => _intProperty;
            set
            {
                _intProperty = value;
                OnPropertyChanged(nameof(HasIntPropertyControl));
                OnPropertyChanged(nameof(IntProperty));
            }
        }
        public bool HasBoolPropertyControl { get; set; } = false;
        public bool BoolProperty { get; set; } = false;
    
        public ObjectViewModel? SelectedComboBoxValue
        {
            get => _selectedComboBoxValue;
            set
            {
                _selectedComboBoxValue = value;
                OnPropertyChanged(nameof(SelectedComboBoxValue));
                if (_selectedComboBoxValue != null)
                {
                    Name = _selectedComboBoxValue.Name;
                    CodeName = _selectedComboBoxValue.CodeName;
                    if (HasIntPropertyControl)
                    {
                        _lastIntPropertyValue = IntProperty;
                    }
                    IntProperty = !value.HasIntPropertyControl ? -1 : _lastIntPropertyValue;
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
        public string CodeName { get; set; } = string.Empty;
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}