using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Newtonsoft.Json.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace E33Randomizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private CustomPlacementWindow _customEnemyPlacementWindow;
    private EditIndividualContainersWindow _editIndividualEncountersWindow;

    private CustomPlacementWindow _customItemPlacementWindow;
    private EditIndividualContainersWindow _editIndividualChecksWindow;
    
    private readonly Dictionary<string, EditIndividualContainersWindow?> _editIndividualContainersWindows = new ();
    private readonly Dictionary<string, CustomPlacementWindow?> _customPlacementWindows = new ();

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
            MessageDialog.Show(this, $"Error starting: {ex.Message}",
                "Loading Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
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

    public async void OpenCustomPlacementButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var objectType = (sender as Button)?.Tag?.ToString();
            if (objectType is null) return;
        
            if (!_customPlacementWindows.TryGetValue(objectType, out CustomPlacementWindow? value) || value == null)
            {
                value = new CustomPlacementWindow(RandomizerLogic.GetCustomPlacement(objectType));
                _customPlacementWindows[objectType] = value;
            
                _customPlacementWindows[objectType]!.Closed += (_, _) => _customPlacementWindows[objectType] = null;
            }

            await value.ShowDialog(this);
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, $"Error Loading Custom Placement Window: {ex.Message}", "Error", nameof(DialogBoxButton.OK),  MessageBoxIcons.Error);
        }
    }

    public async void OpenEditObjectsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var objectType = (sender as Button)?.Tag?.ToString();
            if (objectType is null) return;
            
            if (!_editIndividualContainersWindows.TryGetValue(objectType, out EditIndividualContainersWindow? value) || value == null)
            {
                value = new EditIndividualContainersWindow(Controllers.GetController(objectType), this);
                _editIndividualContainersWindows[objectType] = value;
                _editIndividualContainersWindows[objectType]!.Closed += (_, _) => _editIndividualContainersWindows[objectType] = null;
            }

            await value.ShowDialog(this);
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, $"Error Loading Edit Objects Window: {ex.Message}", "Error", nameof(DialogBoxButton.OK),  MessageBoxIcons.Error);
        }
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        
        try
        {
            RandomizerLogic.Randomize();
            await MessageDialog.ShowAsync(this, $"Generation done! You can find the mod in the rand_{RandomizerLogic.usedSeed} folder.\n\n" +
                                                $"Used Seed: {RandomizerLogic.usedSeed}\n",
                "Generation Summary", nameof(DialogBoxButton.OK), MessageBoxIcons.Information);
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, $"Error generating: {ex.Message}",
                "Generating Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
            await File.WriteAllTextAsync("crash_log.txt", ex.ToString(), Encoding.UTF8);
        }
    }

    
    private async void LoadPresetButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = GetTopLevel(this);
            if (topLevel is null) return;

            var storage = topLevel.StorageProvider;

            var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a File",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("JSON files (*.json)") { Patterns = ["*.json"] },
                    new FilePickerFileType("All Files") { Patterns = ["*"] }
                ]
            });

            if (files.Count == 1)
            {
                try
                {
                    LoadSettings(files[0].Path.LocalPath);
                }
                catch (Exception ex)
                {
                    await MessageDialog.ShowAsync(this, $"Error loading preset: {ex.Message}", 
                        "Load Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
                }
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, $"Error Loading JSON: {ex.Message}", "Error", nameof(DialogBoxButton.OK),  MessageBoxIcons.Error);
        }
    }

    private async void SavePresetButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = GetTopLevel(this);
            if (topLevel is null) return;

            var storage = topLevel.StorageProvider;

            var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = "Save Preset",
                DefaultExtension =  ".json",
                FileTypeChoices = 
                [
                    new FilePickerFileType("JSON files (*.json)") { Patterns = ["*.json"] },
                    new FilePickerFileType("All Files") { Patterns = ["*"] }
                ]
            });

            if (file is not null)
            {
                try
                {
                    SaveSettings(file.Path.LocalPath);
                    await MessageDialog.ShowAsync(this, "Preset saved successfully!", 
                        "Save Complete", nameof(DialogBoxButton.OK), MessageBoxIcons.Information);
                }
                catch (Exception ex)
                {
                    await MessageDialog.ShowAsync(this, $"Error saving preset: {ex.Message}", 
                        "Save Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
                }
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, $"Error Saving JSON: {ex.Message}", "Error", nameof(DialogBoxButton.OK),  MessageBoxIcons.Error);
            await File.WriteAllTextAsync("crash_log.txt", ex.ToString(), Encoding.UTF8);
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
                var newSettingsData = JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.SettingsViewModel) ?? throw new Exception("Invalid settings.");
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
            MessageDialog.Show(this, $"Error loading: {ex.Message}",
                "Loading Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
            File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
        }
    }

    private void SaveSettings(string pathToJson)
    {
        try
        {
            using StreamWriter r = new StreamWriter(pathToJson);
            string json = JsonSerializer.Serialize(RandomizerLogic.Settings, JsonSourceGenerationContextSerializationFactory.LazyJsonSourceGenerationContext.Value.SettingsViewModel);
            r.Write(json);
        }
        catch (Exception ex)
        {
            MessageDialog.Show(this, $"Error saving: {ex.Message}",
                "Saving Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
            File.WriteAllText("crash_log.txt", ex.ToString(), Encoding.UTF8);
        }
    }
}


public class SettingsViewModel : ObservableObject
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
    public bool RandomizeEsquieRocks { get; set; } = false;
    public bool LimitEsquieRandomization { get; set; } = true;

    public bool ChangeMerchantInventorySize { get; set; } = false;
    public int MerchantInventorySizeMax { get; set; } = 15;
    public int MerchantInventorySizeMin { get; set; } = 1;
    
    public bool ChangeItemQuantity { get; set; } = false;
    public int ItemQuantityMax { get; set; } = 10;
    public int ItemQuantityMin { get; set; } = 1;
    
    public bool ChangeMerchantInventoryLocked { get; set; } = false;
    public int MerchantInventoryLockedChancePercent { get; set; } = 10;
    
    public bool ChangeNumberOfLootDrops { get; set; } = false;
    public int LootDropsNumberMax { get; set; } = 3;
    public int LootDropsNumberMin { get; set; } = 0;
    
    public bool ChangeNumberOfTowerRewards { get; set; } = false;
    public int TowerRewardsNumberMax { get; set; } = 3;
    public int TowerRewardsNumberMin { get; set; } = 1;
    
    public bool ChangeNumberOfChestContents { get; set; } = false;
    public int ChestContentsNumberMax { get; set; } = 2;
    public int ChestContentsNumberMin { get; set; } = 1;
    
    public bool ChangeNumberOfActionRewards { get; set; } = false;
    public int ActionRewardsNumberMax { get; set; } = 3;
    public int ActionRewardsNumberMin { get; set; } = 1;
    
    public bool MakeEveryItemVisible { get; set; } = true;
    
    public bool EnsurePaintedPowerFromPaintress { get; set; } = true;
    public bool IncludeGearInPrologue { get; set; } = false;
    public bool RandomizeStartingWeapons { get; set; } = false;
    public bool RandomizeStartingCosmetics { get; set; } = false;
    public bool RandomizeGestralBeachRewards { get; set; } = true;
    public bool RandomizeMonocoFeet { get; set; } = true;
    public bool IncludeCutContentItems { get; set; } = true;
    
    public bool RandomizeSkills { get; set; } = true;
    public bool ReduceSkillRepetition { get; set; } = true;
    public bool IncludeCutContentSkills { get; set; } = false;
    public bool UnlockGustaveSkills { get; set; } = false;
    public bool GuaranteeGustaveOvercharge { get; set; } = true;
    public bool RandomizeSkillUnlockCosts { get; set; } = false;
    public bool RandomizeTreeEdges { get; set; } = true;
    //TODO: Add dummy edge structs to Monoco's asset
    public bool GiveMonocoTreeEdges { get; set; } = false;
    public int MinTreeEdges { get; set; } = 2;
    public int MaxTreeEdges { get; set; } = 4;
    public bool FullyRandomEdges { get; set; } = false;
    public int RandomEdgeChancePercent { get; set; } = 60;
    public bool MakeSkillsIntoItems { get; set; } = false;
    
    public bool RandomizeLocations { get; set; } = true;
    public bool ReduceLocationRepetition { get; set; } = false;
    
    public bool RandomizeCharacters { get; set; } = false;
    
    [System.Text.Json.Serialization.JsonIgnore]
    public int SelectedIndex
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
                UpdateCurrentPage();
            }
        }
    }

    [JsonIgnore]
    public object? CurrentPage
    {
        get => field ?? new RandomizeEnemiesTab();
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    private void UpdateCurrentPage()
    {
        CurrentPage = SelectedIndex switch
        {
            0 => new RandomizeEnemiesTab(),
            1 => new RandomizeItemsTab(),
            2 => new RandomizeSkillsTab(),
            3 => new RandomizeLocationsTab(),
            4 => new MiscTab(),
            _ => new RandomizeEnemiesTab()
        };
    }
}
