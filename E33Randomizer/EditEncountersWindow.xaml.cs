using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace E33Randomizer
{
    public partial class EditEncountersWindow : Window
    {
        public EditEncounterWindowViewModel ViewModel { get; set; }

        public EditEncountersWindow()
        {
            InitializeComponent();
            ViewModel = new EditEncounterWindowViewModel();
            DataContext = ViewModel;
            
            LoadSampleData();
        }

        private void LocationTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is EncounterViewModel selectedEncounter)
            {
                ViewModel.OnEncounterSelected(selectedEncounter);
            }
        }

        private void AddEnemyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddEnemyComboBox.SelectedItem is EnemyViewModel selectedEnemy)
            {
                ViewModel.AddSelectedEnemy(selectedEnemy);
                
                AddEnemyComboBox.SelectedIndex = -1;
            }
        }

        private void RemoveEnemy_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is EnemyViewModel enemyToRemove)
            {
                ViewModel.RemoveSelectedEnemy(enemyToRemove);
            }
        }

        private void LoadSampleData()
        {
            ViewModel.Locations.Add(new EncounterLocationViewModel
            {
                Name = "Spring Meadows",
                Encounters = new ObservableCollection<EncounterViewModel>
                {
                    new EncounterViewModel { CodeName = "SM_Portier", Name = "Portier (Spring Meadows)" }
                }
            });

            ViewModel.Locations.Add(new EncounterLocationViewModel
            {
                Name = "Flying Waters",
                Encounters = new ObservableCollection<EncounterViewModel>
                {
                    new EncounterViewModel { CodeName = "GO_Bruler", Name = "Bruler (Flying Waters)" }
                }
            });

            ViewModel.Locations.Add(new EncounterLocationViewModel
            {
                Name = "Ancient Sanctuary",
                Encounters = new ObservableCollection<EncounterViewModel>
                {
                    new EncounterViewModel { CodeName = "AS_PotatoBag_Mage", Name = "Catapult Sakapatate (Ancient Sanctuary)" }
                }
            });
        }
    }

    public class EditEncounterWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<EncounterLocationViewModel> Locations { get; set; }
        public ObservableCollection<EnemyViewModel> SelectedEnemies { get; set; }
        public ObservableCollection<EnemyViewModel> AvailableEnemies { get; set; }

        public EditEncounterWindowViewModel()
        {
            Locations = new ObservableCollection<EncounterLocationViewModel>();
            SelectedEnemies = new ObservableCollection<EnemyViewModel>();
            AvailableEnemies = new ObservableCollection<EnemyViewModel>();
        }

        public void OnEncounterSelected(EncounterViewModel encounter)
        {
            AvailableEnemies.Clear();
            
            var enemies = GetEnemiesInEncounter(encounter);
            foreach (var enemy in enemies)
            {
                AvailableEnemies.Add(enemy);
            }
        }

        public void AddSelectedEnemy(EnemyViewModel enemy)
        {
            if (!SelectedEnemies.Any(si => si.CodeName == enemy.CodeName))
            {
                SelectedEnemies.Add(new EnemyViewModel { CodeName = enemy.CodeName, Name = enemy.Name });
            }
        }

        public void RemoveSelectedEnemy(EnemyViewModel enemy)
        {
            var enemyToRemove = SelectedEnemies.FirstOrDefault(e => e.CodeName == enemy.CodeName);
            if (enemyToRemove != null)
            {
                SelectedEnemies.Remove(enemyToRemove);
            }
        }

        private List<EnemyViewModel> GetEnemiesInEncounter(EncounterViewModel encounter)
        {
            return new List<EnemyViewModel>
            {
                new EnemyViewModel { CodeName = "Test_PlaceHolderBattleDude", Name = "Place holder battle" },
                new EnemyViewModel { CodeName = "Test_PlaceHolderBattleDude", Name = "Place holder battle" },
                new EnemyViewModel { CodeName = "Test_PlaceHolderBattleDude", Name = "Place holder battle" },
                new EnemyViewModel { CodeName = "Test_PlaceHolderBattleDude", Name = "Place holder battle" }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EncounterLocationViewModel
    {
        public string Name { get; set; }
        public ObservableCollection<EncounterViewModel> Encounters { get; set; }
    }

    public class EncounterViewModel
    {
        public string CodeName { get; set; }
        public string Name { get; set; }
    }

    public class EnemyViewModel
    {
        public string CodeName { get; set; }
        public string Name { get; set; }
    }
}