using System.ComponentModel;
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
    private CustomPlacementWindow _customEnemyPlacementWindow;
    private EditIndividualContainersWindow _editIndividualEncountersWindow;

    private CustomPlacementWindow _customItemPlacementWindow;
    private EditIndividualContainersWindow _editIndividualChecksWindow;

    public MainWindow()
    {
        InitializeComponent();
        try
        {
            RandomizerLogic.Init("Data");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting: {ex.Message}",
                "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        DataContext = RandomizerLogic.Settings;
    }

    private void CustomEnemyPlacementButton_Click(object sender, RoutedEventArgs e)
    {
        if (_customEnemyPlacementWindow == null)
        {
            _customEnemyPlacementWindow = new CustomPlacementWindow(RandomizerLogic.CustomEnemyPlacement)
            {
                Owner = this,
            };
            _customEnemyPlacementWindow.Closed += (_, _) => _customEnemyPlacementWindow = null;
        }

        _customEnemyPlacementWindow.Show();
    }

    private void EditEncountersButton_Click(object sender, RoutedEventArgs e)
    {
        if (_editIndividualEncountersWindow == null)
        {
            _editIndividualEncountersWindow = new EditIndividualContainersWindow("Enemy")
            {
                Owner = this
            };
            _editIndividualEncountersWindow.Closed += (_, _) => _editIndividualEncountersWindow = null;
        }

        _editIndividualEncountersWindow.Show();
    }

    private void CustomItemPlacementButton_Click(object sender, RoutedEventArgs e)
    {
        if (_customItemPlacementWindow == null)
        {
            _customItemPlacementWindow = new CustomPlacementWindow(RandomizerLogic.CustomItemPlacement)
            {
                Owner = this,
            };
            _customItemPlacementWindow.Closed += (_, _) => _customItemPlacementWindow = null;
        }

        _customItemPlacementWindow.Show();
    }

    private void EditChecksButton_Click(object sender, RoutedEventArgs e)
    {
        if (_editIndividualChecksWindow == null)
        {
            _editIndividualChecksWindow = new EditIndividualContainersWindow("Item")
            {
                Owner = this
            };
            _editIndividualChecksWindow.Closed += (_, _) => _editIndividualChecksWindow = null;
        }

        _editIndividualChecksWindow.Show();
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        RandomizerLogic.Randomize();
        MessageBox.Show($"Generation done! You can find the mod in the rand_{RandomizerLogic.usedSeed} folder.\n\n" +
                        $"Used Seed: {RandomizerLogic.usedSeed}\n",
            "Generation Summary", MessageBoxButton.OK, MessageBoxImage.Information);
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

            targetFolder = subdirectories.Length is 0 or > 1 ? saveGamesBase : $"{subdirectories[0]}";
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


public class SettingsViewModel : INotifyPropertyChanged
{
    public int Seed { get; set; } = -1;
    
    public bool RandomizeItems { get; set; } = true;
    public bool RandomizeEnemies { get; set; } = true;
    
    public bool RandomizeEncounterSizes { get; set; } = false;
    public bool ChangeSizeOfNonRandomizedEncounters { get; set; } = false;
    public bool EncounterSizeOne { get; set; } = false;
    public bool EncounterSizeTwo { get; set; } = false;
    public bool EncounterSizeThree { get; set; } = false;
    public bool NoSimonP2BeforeLune { get; set; } = true;
    public string EarliestSimonP2Encounter { get; set; } = "SM_Eveque_ShieldTutorial*1";
    public bool RandomizeMerchantFights { get; set; } = true;
    public bool EnableEnemyOnslaught { get; set; } = false;
    public int EnemyOnslaughtAdditionalEnemies { get; set; } = 1;
    public int EnemyOnslaughtEnemyCap { get; set; } = 4;

    public bool RandomizeAddedEnemies { get; set; } = false;
    public bool EnsureBossesInBossEncounters { get; set; } = false;
    public bool ReduceBossRepetition { get; set; } = false;
    public bool TieDropsToEncounters { get; set; } = false; 

    public bool ChangeSizesOfNonRandomizedChecks { get; set; } = false;
    
    public bool ChangeMerchantInventorySize { get; set; } = false;
    public int MerchantInventorySizeMax { get; set; } = 20;
    public int MerchantInventorySizeMin { get; set; } = 1;
    
    public bool ChangeItemQuantity { get; set; } = false;
    public int ItemQuantityMax { get; set; } = 20;
    public int ItemQuantityMin { get; set; } = 1;
    
    public bool ChangeMerchantInventoryLocked { get; set; } = false;
    public int MerchantInventoryLockedChancePercent { get; set; } = 10;
    
    public bool ChangeNumberOfLootDrops { get; set; } = false;
    public int LootDropsNumberMax { get; set; } = 5;
    public int LootDropsNumberMin { get; set; } = 1;
    
    public bool ChangeNumberOfTowerRewards { get; set; } = false;
    public int TowerRewardsNumberMax { get; set; } = 5;
    public int TowerRewardsNumberMin { get; set; } = 1;
    
    public bool ChangeNumberOfChestContents { get; set; } = false;
    public int ChestContentsNumberMax { get; set; } = 5;
    public int ChestContentsNumberMin { get; set; } = 1;
    
    public bool ChangeNumberOfActionRewards { get; set; } = false;
    public int ActionRewardsNumberMax { get; set; } = 5;
    public int ActionRewardsNumberMin { get; set; } = 1;
    
    public bool MakeEveryItemVisible { get; set; } = true;
    
    public bool EnsurePaintedPowerFromPaintress { get; set; } = true;
    public bool RandomizeStartingWeapons { get; set; } = false;
    public bool RandomizeStartingCosmetics { get; set; } = false;
    public bool RandomizeGestralBeachRewards { get; set; } = true;
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}