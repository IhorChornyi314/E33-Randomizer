using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace E33Randomizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private CustomPlacementWindow _customWindow;
    private EditEncountersWindow _editEncountersWindow;

        public MainWindow()
        {
            InitializeComponent();
            RandomizerLogic.Init();
            // Setup data bindings
            SetupDataBindings();
        }

        private void SetupDataBindings()
        {
            // This is really bad, I'll clean it up soon I promise
            
            RandomizeEncounterSizesCheckBox.IsChecked = Settings.RandomizeEncounterSizes;
            ChangeSizeOfNonRandomEncountersCheckBox.IsChecked = Settings.ChangeSizeOfNonRandomizedEncounters;
            RandomizeMerchantFightsCheckBox.IsChecked = Settings.RandomizeMerchantFights;

            RandomizeEncounterSizesCheckBox.Checked += (_, _) => Settings.RandomizeEncounterSizes = true;
            RandomizeEncounterSizesCheckBox.Unchecked += (_, _) => Settings.RandomizeEncounterSizes = false;
            
            ChangeSizeOfNonRandomEncountersCheckBox.Checked += (_, _) => Settings.ChangeSizeOfNonRandomizedEncounters = true;
            ChangeSizeOfNonRandomEncountersCheckBox.Unchecked += (_, _) => Settings.ChangeSizeOfNonRandomizedEncounters = false;
    
            RandomizeMerchantFightsCheckBox.Checked += (_, _) => Settings.RandomizeMerchantFights = true;
            RandomizeMerchantFightsCheckBox.Unchecked += (_, _) => Settings.RandomizeMerchantFights = false;
    
            IncludeCutContentCheckBox.Checked += (_, _) =>
            {
                if (!RandomizerLogic.CustomEnemyPlacement.Excluded.Contains("Cut Content Enemies"))
                    RandomizerLogic.CustomEnemyPlacement.Excluded.Add("Cut Content Enemies");
            };
            IncludeCutContentCheckBox.Unchecked += (_, _) =>
            {
                if (RandomizerLogic.CustomEnemyPlacement.Excluded.Contains("Cut Content Enemies"))
                    RandomizerLogic.CustomEnemyPlacement.Excluded.Remove("Cut Content Enemies");
            };
            
            EnableEnemyOnslaughtCheckBox.Checked += (_, _) => Settings.EnableEnemyOnslaught = true;
            EnableEnemyOnslaughtCheckBox.Unchecked += (_, _) => Settings.EnableEnemyOnslaught = false;
            
            EncounterSize1CheckBox.Checked += (_, _) => Settings.PossibleEncounterSizes.Add(1);
            EncounterSize1CheckBox.Unchecked += (_, _) => Settings.PossibleEncounterSizes.Remove(1);
            EncounterSize2CheckBox.Checked += (_, _) => Settings.PossibleEncounterSizes.Add(2);
            EncounterSize2CheckBox.Unchecked += (_, _) => Settings.PossibleEncounterSizes.Remove(2);
            EncounterSize3CheckBox.Checked += (_, _) => Settings.PossibleEncounterSizes.Add(3);
            EncounterSize3CheckBox.Unchecked += (_, _) => Settings.PossibleEncounterSizes.Remove(3);

            Phase2SimonComboBox.SelectionChanged += (_, _) =>
                Settings.EarliestSimonP2Encounter = (Phase2SimonComboBox.SelectedItem as ComboBoxItem).Tag.ToString();
            SeedTextBox.TextChanged += (_, _) =>
            {
                SeedTextBox.Text = SeedTextBox.Text.Replace(" ", "");
                if (int.TryParse(SeedTextBox.Text, out int value))
                {
                    Settings.Seed = value;
                }
            };
            
            RandomizeAdditionalEnemiesCheckBox.IsChecked = Settings.RandomizeAddedEnemies;
            EnsureBossesInBossEncountersCheckBox.IsChecked = Settings.EnsureBossesInBossEncounters;
            ReduceBossRepetitionCheckBox.IsChecked = Settings.ReduceBossRepetition;
            // TieLootToEncountersButton.IsChecked = Settings.TieDropsToEncounters;

            RandomizeAdditionalEnemiesCheckBox.Checked += (_, _) => Settings.RandomizeAddedEnemies = true;
            RandomizeAdditionalEnemiesCheckBox.Unchecked += (_, _) => Settings.RandomizeAddedEnemies = false;
            
            EnsureBossesInBossEncountersCheckBox.Checked += (_, _) => Settings.EnsureBossesInBossEncounters = true;
            EnsureBossesInBossEncountersCheckBox.Unchecked += (_, _) => Settings.EnsureBossesInBossEncounters = false;
            
            ReduceBossRepetitionCheckBox.Checked += (_, _) => Settings.ReduceBossRepetition = true;
            ReduceBossRepetitionCheckBox.Unchecked += (_, _) => Settings.ReduceBossRepetition = false;

            // TieLootToEncountersButton.Checked += (_, _) => Settings.TieDropsToEncounters = true;
            // TieLootToEncountersButton.Unchecked += (_, _) => Settings.TieDropsToEncounters = false;
            
            RandomizeItemsCheckBox.Checked += (_, _) => Settings.RandomizeItems = true;
            RandomizeItemsCheckBox.Unchecked += (_, _) => Settings.RandomizeItems = false;
        }
    

        private void CustomEnemyPlacementButton_Click(object sender, RoutedEventArgs e)
        {
            if (_customWindow == null)
            {
                _customWindow = new CustomPlacementWindow(RandomizerLogic.CustomEnemyPlacement)
                {
                    Owner = this,
                };
                _customWindow.Closed += (_, _) => _customWindow = null;
            }
            
            _customWindow.Show();
            //customEnemyWindow.Focus();
        }
        
        private void EditEncountersButton_Click(object sender, RoutedEventArgs e)
        {
            if (_editEncountersWindow == null)
            {
                _editEncountersWindow = new EditEncountersWindow
                {
                    Owner = this
                };
                _editEncountersWindow.Closed += (_, _) => _editEncountersWindow = null;
            }
            
            _editEncountersWindow.Show();
            //customEnemyWindow.Focus();
        }

        private void NumberOfAdditionalEnemiesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumberOfAdditionalEnemiesTextBox.Text = NumberOfAdditionalEnemiesTextBox.Text.Replace(" ", "");
            if (int.TryParse(NumberOfAdditionalEnemiesTextBox.Text, out int value))
            {
                Settings.EnemyOnslaughtAdditionalEnemies = value;
            }
        }
        
        private void EnemyNumberCapTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnemyNumberCapTextBox.Text = EnemyNumberCapTextBox.Text.Replace(" ", "");
            if (int.TryParse(EnemyNumberCapTextBox.Text, out int value))
            {
                Settings.EnemyOnslaughtEnemyCap = value;
            }
        }

        private void SeedTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow numeric input
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        
        private void AdditionalEnemyAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow numeric input
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text) && e.Text.Length < 3;
        }
        
        private void EnemyAmountCap_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow numeric input
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void PresetNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow alphanumeric input
            Regex regex = new Regex("[^a-zA-Z0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            RandomizerLogic.Randomize();
            MessageBox.Show($"Generation done! You can find the mod in the rand_{RandomizerLogic.usedSeed} folder.\n\n" +
                          $"Used Seed: {RandomizerLogic.usedSeed}\n",
                          "Generation Summary", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NumberOfAdditionalEnemiesTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (NumberOfAdditionalEnemiesTextBox.Text == "")
            {
                NumberOfAdditionalEnemiesTextBox.Text = Settings.EnemyOnslaughtAdditionalEnemies.ToString();
            }
        }
        
        private void EnemyNumberCapTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (EnemyNumberCapTextBox.Text == "")
            {
                EnemyNumberCapTextBox.Text = Settings.EnemyOnslaughtEnemyCap.ToString();
            }
        }

        private void PatchSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            string targetFolder = "";
            try
            {
                string saveGamesBase = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Sandfall\\Saved\\SaveGames\\"
                );
                string[] subdirectories = Directory.GetDirectories(saveGamesBase);

                if (subdirectories.Length == 0 || subdirectories.Length > 1)
                {
                    targetFolder = saveGamesBase;
                }
                else
                {
                    targetFolder = $"{subdirectories[0]}";
                }
            }
            catch (DirectoryNotFoundException exception)
            {
            }
            
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = targetFolder,
                Title = "Select save file",
                Filter = "SAV files (*.sav)|*.sav|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    SaveFilePatcher.Patch(openFileDialog.FileName);
                    
                    MessageBox.Show($"Save File Patched!",
                        "Patched", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error patching: {ex.Message}", 
                        "Patching Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
}