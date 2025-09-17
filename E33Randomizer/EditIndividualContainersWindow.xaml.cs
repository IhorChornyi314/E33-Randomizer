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

namespace E33Randomizer
{
    public partial class EditIndividualContainersWindow : Window
    {
        public EditIndividualObjectsWindowViewModel ViewModel { get; set; }
        private ContainerViewModel _selectedContainerViewModel;
        private string _objectType;

        public EditIndividualContainersWindow(string objectType)
        {
            _objectType = objectType;
            InitializeComponent();
            ViewModel = objectType == "Enemy" ? EncountersController.ViewModel : ItemsController.ViewModel;
            DataContext = ViewModel;
            ApplyObjectsType();
        }

        private void ApplyObjectsType()
        {
            (FindName("ContainersTextBlock") as TextBlock).Text = ViewModel.ContainerName + "s";
            (FindName("AddObjectTextBlock") as TextBlock).Text = $"Add {ViewModel.ObjectName.ToLower()}:";
            (FindName("ObjectsTextBlock") as TextBlock).Text = $"{ViewModel.ObjectName}s in the {ViewModel.ContainerName.ToLower()}".Replace("ys", "ies");
            (FindName("LoadTextButton") as Button).Content = $"Load {ViewModel.ContainerName.ToLower()}s from .txt file";
            (FindName("SaveTextButton") as Button).Content = $"Save {ViewModel.ContainerName.ToLower()}s to .txt file";
            (FindName("SearchLabel") as Label).Content = $"Search by {ViewModel.ContainerName.ToLower()} or {ViewModel.ObjectName.ToLower()} names:";
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

        private void AddObjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedContainerViewModel != null && AddObjectComboBox.SelectedItem is ObjectViewModel selectedObject)
            {
                if (_objectType == "Enemy")
                {
                    EncountersController.AddEnemyToEncounter(selectedObject.CodeName, _selectedContainerViewModel.CodeName);
                }
                else if (_objectType == "Item")
                {
                    ItemsController.AddItemToCheck(selectedObject.CodeName, _selectedContainerViewModel.CodeName);
                }
            }
            AddObjectComboBox.SelectedIndex = -1;
        }

        private void RemoveObject_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContainerViewModel != null && sender is Button button && button.Tag is ObjectViewModel selectedObject)
            {
                if (_objectType == "Enemy")
                {
                    EncountersController.RemoveEnemyFromEncounter(selectedObject.Index, _selectedContainerViewModel.CodeName);
                }
                else if (_objectType == "Item")
                {
                    ItemsController.RemoveItemFromCheck(selectedObject.Index, _selectedContainerViewModel.CodeName);
                }
            }
        }

        public void RegenerateData(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (_objectType)
                {
                    case "Enemy":
                        EncountersController.GenerateNewEncounters();
                        break;
                    case "Item":
                        ItemsController.GenerateNewItemChecks();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating: {ex.Message}", 
                    "Reroll Error", MessageBoxButton.OK, MessageBoxImage.Error);
                File.WriteAllText("reroll_error_log.txt", ex.ToString(), Encoding.UTF8);
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
                File.WriteAllText("data_packing_error_log.txt", ex.ToString(), Encoding.UTF8);
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
                    switch (_objectType)
                    {
                        case "Enemy":
                            EncountersController.ReadEncountersTxt(openFileDialog.FileName);
                            break;
                        case "Item":
                            ItemsController.ReadChecksTxt(openFileDialog.FileName);
                            break;
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading TXT: {ex.Message}", 
                        "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    File.WriteAllText("txt_loading_error_log.txt", ex.ToString(), Encoding.UTF8);
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
                    switch (_objectType)
                    {
                        case "Enemy":
                            EncountersController.WriteEncountersTxt(saveFileDialog.FileName);
                            break;
                        case "Item":
                            ItemsController.WriteChecksTxt(saveFileDialog.FileName);
                            break;
                    }
                    MessageBox.Show("TXT saved successfully!", 
                        "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving TXT: {ex.Message}", 
                        "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    File.WriteAllText("txt_saving_error_log.txt", ex.ToString(), Encoding.UTF8);
                }
            }
        }

        private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.SearchTerm = SearchTextBox.Text;
            ViewModel.UpdateFilteredCategories();
        }
    }

    public class EditIndividualObjectsWindowViewModel : INotifyPropertyChanged
    {
        public string ContainerName { get; set; }
        public string ObjectName { get; set; }
        public List<CategoryViewModel> Categories { get; set; }
        public ObservableCollection<CategoryViewModel> FilteredCategories { get; set; }
        public ObservableCollection<ObjectViewModel> DisplayedObjects { get; set; }
        public ObservableCollection<ObjectViewModel> AllObjects { get; set; }
        public ContainerViewModel CurrentContainer = null;
        public string SearchTerm = "";
        public bool CanAddObjects { get; set; }  = true;
        

        public EditIndividualObjectsWindowViewModel()
        {
            DisplayedObjects = [];
            FilteredCategories = [];
            Categories = [];
            AllObjects = [];
        }

        public void UpdateFilteredCategories()
        {
            FilteredCategories.Clear();

            foreach (var category in Categories)
            {
                var newCategory = new CategoryViewModel();
                newCategory.CategoryName = category.CategoryName;
                newCategory.Containers = new ObservableCollection<ContainerViewModel>(category.Containers.OrderBy(c => c.Name).Where(c => 
                        c.Name.ToLower().Contains(SearchTerm.ToLower()) ||
                        c.CodeName.ToLower().Contains(SearchTerm.ToLower()) ||
                        c.Objects.Any(o => o.CodeName.ToLower().Contains(SearchTerm.ToLower()) || o.Name.ToLower().Contains(SearchTerm.ToLower())
                        )
                    )
                );
                if (newCategory.Containers.Count > 0)
                {
                    FilteredCategories.Add(newCategory);
                }
            }
        }

        public void UpdateDisplayedObjects()
        {
            DisplayedObjects.Clear();
            foreach (var objectViewModel in CurrentContainer.Objects)
            {
                objectViewModel.InitComboBox(AllObjects);
                DisplayedObjects.Add(objectViewModel);
            }
        }

        public void OnContainerSelected(ContainerViewModel container)
        {
            CurrentContainer = container;
            CanAddObjects = !container.CodeName.Contains("BP_Dialog");
            OnPropertyChanged(nameof(CanAddObjects));
            UpdateDisplayedObjects();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CategoryViewModel
    {
        public string CategoryName { get; set; }
        public ObservableCollection<ContainerViewModel> Containers { get; set; }
    }

    public class ContainerViewModel
    {
        public ContainerViewModel(string containerCodeName, string containerCustomName="")
        {
            CodeName = containerCodeName;
            Name = containerCustomName == "" ? containerCodeName : containerCustomName;
        }
        public string CodeName { get; set; }
        public string Name { get; set; }
        
        public ObservableCollection<ObjectViewModel> Objects { get; set; }
    }

    public class ObjectViewModel : INotifyPropertyChanged
    {
        private ObjectViewModel _selectedComboBoxValue;
        private int _lastItemQuantity = 1;
        public string Name { get; set; }
        public ObservableCollection<ObjectViewModel> AllObjects { get; set; } = [];
        public bool CanDelete { get; set; } = true;

        public bool HasQuantityControl => ItemQuantity != -1;
        private int _itemQuantity = -1;

        public int ItemQuantity
        {
            get => _itemQuantity;
            set
            {
                _itemQuantity = value;
                OnPropertyChanged(nameof(HasQuantityControl));
                OnPropertyChanged(nameof(ItemQuantity));
            }
        }
        public bool IsMerchantInventory { get; set; } = false;
        public bool MerchantInventoryLocked { get; set; } = false;
    
        public ObjectViewModel SelectedComboBoxValue
        {
            get => _selectedComboBoxValue;
            set
            {
                _selectedComboBoxValue = value;
                OnPropertyChanged(nameof(SelectedComboBoxValue));
                Name = _selectedComboBoxValue.Name;
                CodeName = _selectedComboBoxValue.CodeName;
                if (ItemsController.ItemsWithQuantities.Contains(CodeName))
                {
                    _lastItemQuantity = ItemQuantity;
                }
                ItemQuantity = !ItemsController.ItemsWithQuantities.Contains(CodeName) ? -1 : _lastItemQuantity;
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
        public string CodeName { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

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