using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace E33Randomizer
{
    public partial class EditEncountersWindow : Window
    {
        public EditEncounterWindowViewModel ViewModel { get; set; }
        private EncounterViewModel _selectedEncounterViewModel;

        public EditEncountersWindow()
        {
            InitializeComponent();
            ViewModel = new EditEncounterWindowViewModel();
            DataContext = ViewModel;
        }
        
        private void LocationTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is EncounterViewModel selectedEncounter)
            {
                _selectedEncounterViewModel = selectedEncounter;
                ViewModel.OnEncounterSelected(selectedEncounter);
            }
        }

        private void AddEnemyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedEncounterViewModel != null && AddEnemyComboBox.SelectedItem is EnemyViewModel selectedEnemy)
            {
                EncountersController.AddEnemyToEncounter(selectedEnemy.CodeName, _selectedEncounterViewModel.CodeName);
                
                ViewModel.UpdateEncounterEnemies(_selectedEncounterViewModel);
            }
            AddEnemyComboBox.SelectedIndex = -1;
        }

        private void RemoveEnemy_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEncounterViewModel != null && sender is Button button && button.Tag is EnemyViewModel enemyToRemove)
            {
                EncountersController.RemoveEnemyFromEncounter(enemyToRemove.CodeName, _selectedEncounterViewModel.CodeName);
                
                ViewModel.UpdateEncounterEnemies(_selectedEncounterViewModel);
            }
        }

        public void RegenerateEncounters(object sender, RoutedEventArgs e)
        {
            EncountersController.GenerateNewEncounters();
            ViewModel.UpdateEncounterEnemies(_selectedEncounterViewModel);
        }
        
        public void PackCurrentEncounters(object sender, RoutedEventArgs e)
        {
            RandomizerLogic.usedSeed = Settings.Seed != -1 ? Settings.Seed : Environment.TickCount; 
            
            RandomizerLogic.PackAndConvertData();
            MessageBox.Show($"Generation done! You can find the mod in the rand_{RandomizerLogic.usedSeed} folder.\n\n" +
                            $"Used Seed: {RandomizerLogic.usedSeed}\n",
                "Generation Summary", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ReadEncountersFromTxt(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Load Custom Preset",
                Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    EncountersController.ReadEncountersTxt(openFileDialog.FileName);
                    ViewModel.UpdateEncounterEnemies(_selectedEncounterViewModel);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading preset: {ex.Message}", 
                        "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void SaveEncountersAsTxt(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save Custom Preset",
                Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                DefaultExt = "txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    EncountersController.WriteEncountersTxt(saveFileDialog.FileName);
                    MessageBox.Show("Encounters saved successfully!", 
                        "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving encounters: {ex.Message}", 
                        "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }

    public class EditEncounterWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<EncounterLocationViewModel> Locations { get; set; }
        public ObservableCollection<EnemyViewModel> EnemiesInTheEncounter { get; set; }
        public ObservableCollection<EnemyViewModel> AllEnemies { get; set; }

        public EditEncounterWindowViewModel()
        {
            EnemiesInTheEncounter = new ObservableCollection<EnemyViewModel>();
            
            Locations = new ObservableCollection<EncounterLocationViewModel>();
            var encountersByLocation = EncountersController.GetEncountersByLocation();
            foreach (var locationEncounterPair in encountersByLocation)
            {
                var newLocationViewModel = new EncounterLocationViewModel();
                newLocationViewModel.LocationName = locationEncounterPair.Key;
                newLocationViewModel.Encounters = new ObservableCollection<EncounterViewModel>();
                foreach (var encounterData in locationEncounterPair.Value)
                {
                    newLocationViewModel.Encounters.Add(new EncounterViewModel(encounterData));
                }
                Locations.Add(newLocationViewModel);
            }
            
            Locations = new ObservableCollection<EncounterLocationViewModel>(Locations.OrderBy(l => l.LocationName));
            AllEnemies = new ObservableCollection<EnemyViewModel>();

            foreach (var enemyData in EnemiesController.enemies)
            {
                AllEnemies.Add(new EnemyViewModel(enemyData));
            }
        }

        public void UpdateEncounterEnemies(EncounterViewModel encounter)
        {
            if (encounter == null)
            {
                return;
            }
            
            EnemiesInTheEncounter.Clear();
            
            var enemies = GetEnemiesInEncounter(encounter);
            foreach (var enemy in enemies)
            {
                EnemiesInTheEncounter.Add(enemy);
            }
        }

        public void OnEncounterSelected(EncounterViewModel encounter)
        {
            UpdateEncounterEnemies(encounter);
        }

        private List<EnemyViewModel> GetEnemiesInEncounter(EncounterViewModel encounter)
        {
            var result = new List<EnemyViewModel>();
            var encounterData = EncountersController.Encounters.Find(e => e.Name == encounter.CodeName);
            if (encounterData == null)
            {
                return result;
            }

            foreach (var enemyData in encounterData.Enemies)
            {
                var enemyViewModel = new EnemyViewModel(enemyData);
                result.Add(enemyViewModel);
            }

            return result;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EncounterLocationViewModel
    {
        public string LocationName { get; set; }
        public ObservableCollection<EncounterViewModel> Encounters { get; set; }
    }

    public class EncounterViewModel
    {
        public EncounterViewModel(Encounter encounterData)
        {
            CodeName = encounterData.Name;
            Name = encounterData.Name;
        }
        public string CodeName { get; set; }
        public string Name { get; set; }
    }

    public class EnemyViewModel
    {
        public EnemyViewModel(EnemyData enemyData)
        {
            CodeName = enemyData.CodeName;
            Name = enemyData.CustomName;
        }
        public string CodeName { get; set; }
        public string Name { get; set; }
    }
}