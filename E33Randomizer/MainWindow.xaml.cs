using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    
    private Dictionary<string, EditIndividualContainersWindow> _editIndividualContainersWindows = new ();
    private Dictionary<string, CustomPlacementWindow> _customPlacementWindows = new ();

    public MainWindow()
    {
        InitializeComponent();
        try
        {
            RandomizerLogic.Init();
            DataContext = RandomizerLogic.Settings;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting: {ex.Message}",
                "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
        }
        if (File.Exists("default_settings.json"))
        {
            LoadSettings("default_settings.json");
        }
        else
        {
            SaveSettings("default_settings.json");
        }
    }

    public void OpenCustomPlacementButton_Click(object sender, RoutedEventArgs e)
    {
        var objectType = (sender as Button).Tag.ToString();
        if (!_customPlacementWindows.ContainsKey(objectType) || _customPlacementWindows[objectType] == null)
        {
            _customPlacementWindows[objectType] = new CustomPlacementWindow(RandomizerLogic.GetCustomPlacement(objectType))
            {
                Owner = this
            };
            _customPlacementWindows[objectType].Closed += (_, _) => _customPlacementWindows[objectType] = null;
        }

        _customPlacementWindows[objectType].Show();
    }

    public void OpenEditObjectsButton_Click(object sender, RoutedEventArgs e)
    {
        var objectType = (sender as Button).Tag.ToString();
        if (!_editIndividualContainersWindows.ContainsKey(objectType) || _editIndividualContainersWindows[objectType] == null)
        {
            _editIndividualContainersWindows[objectType] = new EditIndividualContainersWindow(Controllers.GetController(objectType))
            {
                Owner = this
            };
            _editIndividualContainersWindows[objectType].Closed += (_, _) => _editIndividualContainersWindows[objectType] = null;
        }

        _editIndividualContainersWindows[objectType].Show();
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        
        try
        {
            RandomizerLogic.Randomize();
            MessageBox.Show($"Generation done! You can find the mod in the rand_{RandomizerLogic.usedSeed} folder.\n\n" +
                            $"Used Seed: {RandomizerLogic.usedSeed}\n",
                "Generation Summary", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating: {ex.Message}",
                "Generating Error", MessageBoxButton.OK, MessageBoxImage.Error);
            File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
        }
    }

    private void EnableCountersInSaveButton_OnClick(object sender, RoutedEventArgs e)
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
                switch ((sender as Button).Tag as string)
                {
                    case "AddCounters":
                        SaveFilePatcher.AddCounters(openFileDialog.FileName);
                        break;
                    case "FixCurtain":
                        SaveFilePatcher.FixCurtain(openFileDialog.FileName);
                        break;
                }
                
                MessageBox.Show($"Save File Patched!",
                    "Patched", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error patching: {ex.Message}",
                    "Patching Error", MessageBoxButton.OK, MessageBoxImage.Error);
                File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
            }
        }
    }
    
    private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Title = "Load Preset",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            FilterIndex = 1
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                LoadSettings(openFileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading preset: {ex.Message}", 
                    "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void SavePresetButton_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Title = "Save Preset",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            FilterIndex = 1,
            DefaultExt = "json"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                SaveSettings(saveFileDialog.FileName);
                MessageBox.Show("Preset saved successfully!", 
                    "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving preset: {ex.Message}", 
                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private bool AllSettingsInJson(string json)
    {
        JObject obj = JObject.Parse(json);
        var jsonProps = obj.Properties().Select(p => p.Name).ToHashSet();
        var classProps = typeof(SettingsViewModel).GetProperties().Select(p => p.Name).ToHashSet();

        return classProps.All(p => jsonProps.Contains(p));
    }

    private void LoadSettings(string pathToJson)
    {
        try
        {
            string json;
            using (StreamReader r = new StreamReader(pathToJson))
            {
                json = r.ReadToEnd();
                var newSettingsData = JsonConvert.DeserializeObject<SettingsViewModel>(json);
                RandomizerLogic.Settings = newSettingsData;
                DataContext = RandomizerLogic.Settings;
            }
            
            if (!AllSettingsInJson(json))
            {
                SaveSettings(pathToJson);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading: {ex.Message}",
                "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
        }
    }

    private void SaveSettings(string pathToJson)
    {
        try
        {
            using StreamWriter r = new StreamWriter(pathToJson);
            string json = JsonConvert.SerializeObject(RandomizerLogic.Settings, Formatting.Indented);
            r.Write(json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving: {ex.Message}",
                "Saving Error", MessageBoxButton.OK, MessageBoxImage.Error);
            File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
        }
    }
}


public class SettingsViewModel : INotifyPropertyChanged
{
    public int Seed { get; set; } = -1;
    
    public bool RandomizeEnemies { get; set; } = true;
    
    public bool RandomizeEncounterSizes { get; set; } = false;
    public bool ChangeSizeOfNonRandomizedEncounters { get; set; } = false;
    public bool EncounterSizeOne { get; set; } = false;
    public bool EncounterSizeTwo { get; set; } = false;
    public bool EncounterSizeThree { get; set; } = false;
    public bool NoSimonP2BeforeLune { get; set; } = true;
    public bool RandomizeMerchantFights { get; set; } = true;
    public bool EnableEnemyOnslaught { get; set; } = false;
    public int EnemyOnslaughtAdditionalEnemies { get; set; } = 1;
    public int EnemyOnslaughtEnemyCap { get; set; } = 4;
    public bool IncludeCutContentEnemies { get; set; } = true;

    public bool RandomizeAddedEnemies { get; set; } = false;
    public bool EnsureBossesInBossEncounters { get; set; } = false;
    public bool ReduceBossRepetition { get; set; } = false;
    // public bool TieDropsToEncounters { get; set; } = false; 
    

    public bool RandomizeItems { get; set; } = true;
    public bool ChangeSizesOfNonRandomizedChecks { get; set; } = false;
    
    public bool ReduceKeyItemRepetition { get; set; } = true;
    public bool ReduceGearRepetition { get; set; } = true;

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
    public bool IncludeGearInPrologue { get; set; } = false;
    public bool RandomizeStartingWeapons { get; set; } = false;
    public bool RandomizeStartingCosmetics { get; set; } = false;
    public bool RandomizeGestralBeachRewards { get; set; } = true;
    public bool IncludeCutContentItems { get; set; } = true;
    
    public bool RandomizeSkills { get; set; } = false;
    public bool ReduceSkillRepetition { get; set; } = true;
    public bool IncludeCutContentSkills { get; set; } = false;
    public bool UnlockGustaveSkills { get; set; } = false;
    public bool RandomizeSkillUnlockCosts { get; set; } = false;
    public bool RandomizeTreeEdges { get; set; } = true;
    //TODO: Add dummy edge structs to Monoco's asset
    public bool GiveMonocoTreeEdges { get; set; } = false;
    public int MinTreeEdges { get; set; } = 2;
    public int MaxTreeEdges { get; set; } = 4;
    public bool FullyRandomEdges { get; set; } = false;
    public int RandomEdgeChancePercent { get; set; } = 60;
    public bool AddSkillsAsItems { get; set; } = false;
    
    public bool RandomizeCharacters { get; set; } = false;
    
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}