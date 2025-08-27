using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        private void RemoveEnemy_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContainerViewModel != null && sender is Button button && button.Tag is ObjectViewModel selectedObject)
            {
                if (_objectType == "Enemy")
                {
                    EncountersController.RemoveEnemyFromEncounter(selectedObject.CodeName, _selectedContainerViewModel.CodeName);
                }
                else if (_objectType == "Item")
                {
                    ItemsController.RemoveItemFromCheck(selectedObject.CodeName, _selectedContainerViewModel.CodeName);
                }
            }
        }

        public void RegenerateData(object sender, RoutedEventArgs e)
        {
            if (_objectType == "Enemy")
            {
                EncountersController.GenerateNewEncounters();
            }
            else if (_objectType == "Item")
            {
                ItemsController.GenerateNewItemChecks();
            }
            
        }
        
        public void PackCurrentData(object sender, RoutedEventArgs e)
        {
            RandomizerLogic.usedSeed = Settings.Seed != -1 ? Settings.Seed : Environment.TickCount; 
            
            RandomizerLogic.PackAndConvertData();
            MessageBox.Show($"Generation done! You can find the mod in the rand_{RandomizerLogic.usedSeed} folder.\n\n" +
                            $"Used Seed: {RandomizerLogic.usedSeed}\n",
                "Generation Summary", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    EncountersController.ReadEncountersTxt(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading TXT: {ex.Message}", 
                        "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    EncountersController.WriteEncountersTxt(saveFileDialog.FileName);
                    MessageBox.Show("TXT saved successfully!", 
                        "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving TXT: {ex.Message}", 
                        "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        public List<CategoryViewModel> Categories { get; set; }
        public ObservableCollection<CategoryViewModel> FilteredCategories { get; set; }
        public ObservableCollection<ObjectViewModel> DisplayedObjects { get; set; }
        public ObservableCollection<ObjectViewModel> AllObjects { get; set; }
        public ContainerViewModel CurrentContainer = null;
        public string SearchTerm = "";

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
                newCategory.Containers = new ObservableCollection<ContainerViewModel>(category.Containers.Where(c => 
                        c.Name.ToLower().Contains(SearchTerm.ToLower()) ||
                        c.CodeName.ToLower().Contains(SearchTerm.ToLower()) ||
                        c.Objects.Any(o => o.CodeName.ToLower().Contains(SearchTerm.ToLower()) || o.Name.ToLower().Contains(SearchTerm.ToLower()))
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
                DisplayedObjects.Add(objectViewModel);
            }
        }

        public void OnContainerSelected(ContainerViewModel container)
        {
            CurrentContainer = container;
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

    public class ObjectViewModel
    {
        public ObjectViewModel(ObjectData objectData)
        {
            CodeName = objectData.CodeName;
            Name = objectData.CustomName;
        }
        public string CodeName { get; set; }
        public string Name { get; set; }
    }
}