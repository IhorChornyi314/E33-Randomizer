using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

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
            
            EncounterSize1CheckBox.Checked += (s, e) => Settings.PossibleEncounterSizes.Add(1);
            EncounterSize1CheckBox.Unchecked += (s, e) => Settings.PossibleEncounterSizes.Remove(1);
            EncounterSize2CheckBox.Checked += (s, e) => Settings.PossibleEncounterSizes.Add(2);
            EncounterSize2CheckBox.Unchecked += (s, e) => Settings.PossibleEncounterSizes.Remove(2);
            EncounterSize3CheckBox.Checked += (s, e) => Settings.PossibleEncounterSizes.Add(3);
            EncounterSize3CheckBox.Unchecked += (s, e) => Settings.PossibleEncounterSizes.Remove(3);

            Phase2SimonComboBox.SelectionChanged += (s, e) =>
                Settings.EarliestSimonP2Encounter = (Phase2SimonComboBox.SelectedItem as ComboBoxItem).Tag.ToString();
            SeedTextBox.TextChanged += (sender, args) => RandomizerLogic.usedSeed = int.Parse(SeedTextBox.Text);
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

        private void SeedTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
            // Handle generation logic here
            MessageBox.Show("Generation done!\n\n" +
                          $"Randomize Encounter Sizes: {Settings.RandomizeEncounterSizes}\n" +
                          $"Randomize Merchant Fights: {Settings.RandomizeMerchantFights}\n" +
                          $"Include Cut Content: {Settings.IncludeCutContent}\n" +
                          $"Seed: {RandomizerLogic.usedSeed}\n",
                          "Settings Summary", MessageBoxButton.OK, MessageBoxImage.Information);
        }
}