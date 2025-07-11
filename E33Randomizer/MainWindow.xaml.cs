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
public partial class MainWindow : Window
{
    private CustomEnemyPlacementWindow customEnemyWindow;

        public MainWindow()
        {
            InitializeComponent();
            RandomizerLogic.Init();
            
            // Setup data bindings
            SetupDataBindings();
        }

        private void SetupDataBindings()
        {
            RandomizeEncounterSizesCheckBox.IsChecked = Settings.RandomizeEncounterSizes;
            RandomizeMerchantFightsCheckBox.IsChecked = Settings.RandomizeMerchantFights;
            IncludeCutContentCheckBox.IsChecked = Settings.IncludeCutContent;

            RandomizeEncounterSizesCheckBox.Checked += (s, e) => Settings.RandomizeEncounterSizes = true;
            RandomizeEncounterSizesCheckBox.Unchecked += (s, e) => Settings.RandomizeEncounterSizes = false;
    
            RandomizeMerchantFightsCheckBox.Checked += (s, e) => Settings.RandomizeMerchantFights = true;
            RandomizeMerchantFightsCheckBox.Unchecked += (s, e) => Settings.RandomizeMerchantFights = false;
    
            IncludeCutContentCheckBox.Checked += (s, e) => Settings.IncludeCutContent = true;
            IncludeCutContentCheckBox.Unchecked += (s, e) => Settings.IncludeCutContent = false;
            
            EnableEnemyOnslaughtCheckBox.Checked += (s, e) => Settings.EnableEnemyOnslaught = true;
            EnableEnemyOnslaughtCheckBox.Unchecked += (s, e) => Settings.EnableEnemyOnslaught = false;
            
            EncounterSize1CheckBox.Checked += (s, e) => Settings.PossibleEncounterSizes.Add(1);
            EncounterSize1CheckBox.Unchecked += (s, e) => Settings.PossibleEncounterSizes.Remove(1);
            EncounterSize2CheckBox.Checked += (s, e) => Settings.PossibleEncounterSizes.Add(2);
            EncounterSize2CheckBox.Unchecked += (s, e) => Settings.PossibleEncounterSizes.Remove(2);
            EncounterSize3CheckBox.Checked += (s, e) => Settings.PossibleEncounterSizes.Add(3);
            EncounterSize3CheckBox.Unchecked += (s, e) => Settings.PossibleEncounterSizes.Remove(3);

            Phase2SimonComboBox.SelectionChanged += (s, e) =>
                Settings.EarliestSimonP2Encounter = (Phase2SimonComboBox.SelectedItem as ComboBoxItem).Tag.ToString();
            SeedTextBox.TextChanged += (sender, args) =>
            {
                SeedTextBox.Text = SeedTextBox.Text.Replace(" ", "");
                if (int.TryParse(SeedTextBox.Text, out int value))
                {
                    Settings.Seed = value;
                }
            };
            
            
            BossNumberCapCheckBox.IsChecked = Settings.BossNumberCapped;
            EnsureBossesInBossEncountersCheckBox.IsChecked = Settings.EnsureBossesInBossEncounters;
            ReduceBossRepetitionCheckBox.IsChecked = Settings.ReduceBossRepetition;

            BossNumberCapCheckBox.Checked += (sender, args) => Settings.BossNumberCapped = true;
            BossNumberCapCheckBox.Unchecked += (sender, args) => Settings.BossNumberCapped = false;
            
            EnsureBossesInBossEncountersCheckBox.Checked += (sender, args) => Settings.EnsureBossesInBossEncounters = true;
            EnsureBossesInBossEncountersCheckBox.Unchecked += (sender, args) => Settings.EnsureBossesInBossEncounters = false;
            
            ReduceBossRepetitionCheckBox.Checked += (sender, args) => Settings.ReduceBossRepetition = true;
            ReduceBossRepetitionCheckBox.Unchecked += (sender, args) => Settings.ReduceBossRepetition = false;

        }
    

        private void CustomEnemyPlacementButton_Click(object sender, RoutedEventArgs e)
        {
            if (customEnemyWindow == null)
            {
                customEnemyWindow = new CustomEnemyPlacementWindow();
                customEnemyWindow.Owner = this;
                customEnemyWindow.Closed += (s, args) => customEnemyWindow = null;
            }
            
            customEnemyWindow.Show();
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

        private void GenerateFromReportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Load Generation Report",
                Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    RandomizerLogic.GenerateFromReport(openFileDialog.FileName);
                    
                    MessageBox.Show($"Files restored! You can find them in the rand_{RandomizerLogic.usedSeed} folder.",
                        "Restored", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading report: {ex.Message}", 
                        "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }    
        
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            RandomizerLogic.Randomize();
            MessageBox.Show($"Generation done! You can find the mod in the rand_{RandomizerLogic.usedSeed} folder.\n\n" +
                          $"Randomize Encounter Sizes: {Settings.RandomizeEncounterSizes}\n" +
                          $"Randomize Merchant Fights: {Settings.RandomizeMerchantFights}\n" +
                          $"Include Cut Content: {Settings.IncludeCutContent}\n" +
                          $"Enemy Onslaught Enabled: {Settings.EnableEnemyOnslaught}\n" +
                          $"Seed: {RandomizerLogic.usedSeed}\n",
                          "Settings Summary", MessageBoxButton.OK, MessageBoxImage.Information);
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
            string targetFolder;
            string saveGamesBase = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Sandfall\\Saved\\SaveGames\\"
            );
            string[] subdirs = Directory.GetDirectories(saveGamesBase);

            if (subdirs.Length == 0 ||  subdirs.Length > 1)
            {
                targetFolder = saveGamesBase;
            }
            else
            {
                targetFolder = $"{subdirs[0]}";
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