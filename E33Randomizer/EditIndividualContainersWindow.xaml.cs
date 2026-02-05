using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Data;

namespace E33Randomizer
{
    public partial class EditIndividualContainersWindow : Window
    {
        public EditIndividualObjectsWindowViewModel ViewModel { get; set; }
        public BaseController Controller { get; set; }
        private ContainerViewModel? _selectedContainerViewModel = null;
        private string? _objectType = null;

        public EditIndividualContainersWindow(BaseController controller)
        {
            Controller = controller;
            InitializeComponent();
            ViewModel = controller.ViewModel;
            DataContext = ViewModel;
            ApplyObjectsType();
        }

        private void ApplyObjectsType()
        {
            var containersTextBlock = FindName("ContainersTextBlock") as TextBlock;
            if (containersTextBlock != null) containersTextBlock.Text = ViewModel.ContainerName + "s";
            var addObjectTextBlock = FindName("AddObjectTextBlock") as TextBlock;
            if (addObjectTextBlock != null) addObjectTextBlock.Text = $"Add {ViewModel.ObjectName.ToLower()}:";
            var objectsTextBlock = FindName("ObjectsTextBlock") as TextBlock;
            if (objectsTextBlock != null) objectsTextBlock.Text = $"{ViewModel.ObjectName}s in the {ViewModel.ContainerName.ToLower()}".Replace("ys", "ies");
            var loadTextButton = FindName("LoadTextButton") as Button;
            if (loadTextButton != null) loadTextButton.Content = $"Load {ViewModel.ContainerName.ToLower()}s from .txt file";
            var saveTextButton = FindName("SaveTextButton") as Button;
            if (saveTextButton != null) saveTextButton.Content = $"Save {ViewModel.ContainerName.ToLower()}s to .txt file";
            var searchLabel = FindName("SearchLabel") as Label;
            if (searchLabel != null) searchLabel.Content = $"Search by {ViewModel.ContainerName.ToLower()} or {ViewModel.ObjectName.ToLower()} names:";
            Title = $"Edit individual {ViewModel.ContainerName.ToLower()}s";
        }
        
        private void CategoryTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ContainerViewModel selectedContainer)
            {
                _selectedContainerViewModel = selectedContainer;
                ViewModel.OnContainerSelected(selectedContainer);
            }
        }

        private void AddObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContainerViewModel == null) return;

            var selected = ViewModel.SelectedAddObject;
            if (selected == null) return;

            Controller.AddObjectToContainer(selected.CodeName, _selectedContainerViewModel.CodeName);

            // Clear selection + filter (and keep UI consistent)
            ViewModel.SelectedAddObject = null;
            ViewModel.AddObjectFilterTerm = string.Empty;

            // Optional: keep focus on the textbox for rapid adding
            if (FindName("AddObjectComboBox") is ComboBox cb)
                cb.Focus();
        }

        private void RemoveObject_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContainerViewModel != null && sender is Button button && button.Tag is ObjectViewModel selectedObject)
            {
                Controller.RemoveObjectFromContainer(selectedObject.Index, _selectedContainerViewModel.CodeName);
            }
        }

        public void RegenerateData(object sender, RoutedEventArgs e)
        {
            try
            {
                Controller.Randomize();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating: {ex.Message}", 
                    "Reroll Error", MessageBoxButton.OK, MessageBoxImage.Error);
                File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
            }
        }
        
        public void PackCurrentData(object sender, RoutedEventArgs e)
        {
            RandomizerLogic.usedSeed = RandomizerLogic.Settings.Seed != -1 ? RandomizerLogic.Settings.Seed : Environment.TickCount; 
            
            try
            {
                RandomizerLogic.PackAndConvertData();
                MessageBox.Show($"Generation done! You can find the mod in the rand_{RandomizerLogic.usedSeed} folder.\n\n" +
                                $"Used Seed: {RandomizerLogic.usedSeed}\n",
                    "Generation Summary", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error packing: {ex.Message}", 
                    "Packing Error", MessageBoxButton.OK, MessageBoxImage.Error);
                File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
            }
        }

        public void ReadDataFromTxt(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Load TXT",
                Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Controller.ReadTxt(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading TXT: {ex.Message}", 
                        "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
                }
            }
        }

        public void SaveDataAsTxt(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save TXT",
                Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                DefaultExt = "txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    Controller.WriteTxt(saveFileDialog.FileName);
                    MessageBox.Show("TXT saved successfully!", 
                        "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving TXT: {ex.Message}", 
                        "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
                }
            }
        }

        private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTextBox = FindName("SearchTextBox") as TextBox;
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
            AddObjectsView = new ListCollectionView(_allObjects);
            AddObjectsView.Filter = AddObjectsFilter;
            OnPropertyChanged(nameof(AddObjectsView));

            RefreshAddObjectsFilter();
        }
    }

    // This is what the Add ComboBox binds to
    private ICollectionView _addObjectsView;
    public ICollectionView AddObjectsView
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
        AddObjectsView = new ListCollectionView(AllObjects);
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