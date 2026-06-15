using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using E33Randomizer.RandomizationLogic;

namespace E33Randomizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static readonly string Version = ThisAssembly.Info.InformationalVersion.Split('+')[0];
    public const double TabStripWidth = 325;
    public const double TabStripHeight = 850;
    public const double TabContainerWidth = 700;
    public const double TabContainerHeight = 850;
    
    private readonly Dictionary<string, EditIndividualContainersWindow?> _editIndividualContainersWindows = new ();
    private readonly Dictionary<string, CustomPlacements.CustomPlacementWindow?> _customPlacementWindows = new ();

    public MainWindow()
    {
        Loaded += OnLoaded;
        InitializeComponent();
        Width = TabStripWidth +  TabContainerWidth + 20;
        Height = TabContainerHeight;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            RandomizerLogic.Init();
            DataContext = RandomizerLogic.Settings;
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorStarting),ex.Message),
                ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorLoading),null,null), MessageBoxButtons.Ok, MessageBoxIcons.Error);
            await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
        }
        if (File.Exists("default_settings.json"))
        {
            await LoadSettingsAsync("default_settings.json");
        }
        else
        {
            await SaveSettingsAsync("default_settings.json");
        }
    }

    public async void OpenCustomPlacementButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var objectType = (sender as Button)?.Tag?.ToString();
            if (objectType is null) return;
        
            if (!_customPlacementWindows.TryGetValue(objectType, out CustomPlacements.CustomPlacementWindow? value) || value == null)
            {
                var customPlacement = RandomizerLogic.GetCustomPlacement(objectType);
                if (customPlacement is null)
                {
                    throw new Exception(ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_UnableToFindCustomPlacementWindow),objectType));
                }
                
                value = new CustomPlacements.CustomPlacementWindow(customPlacement);
                _customPlacementWindows[objectType] = value;
            
                _customPlacementWindows[objectType]!.Closed += (_, _) => _customPlacementWindows[objectType] = null;
            }

            await value.ShowDialog(this);
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorLoadingCustomPlacementWindow),ex.Message), ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_Error)), MessageBoxButtons.Ok,  MessageBoxIcons.Error);
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
                var controller = Controllers.GetController(objectType);
                if (controller is null)
                {
                    throw new Exception(ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_UnableToFindEditObjectController),objectType));
                }
                value = new EditIndividualContainersWindow(controller, this);
                _editIndividualContainersWindows[objectType] = value;
                _editIndividualContainersWindows[objectType]!.Closed += (_, _) => _editIndividualContainersWindows[objectType] = null;
            }

            await value.ShowDialog(this);
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorLoadingEditObjectsWindow),ex.Message), ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_Error)), MessageBoxButtons.Ok,  MessageBoxIcons.Error);
        }
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            RandomizerLogic.Randomize();
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_GenerationDone),RandomizerLogic.usedSeed),
                ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_GenerationSummary)), MessageBoxButtons.Ok, MessageBoxIcons.Information);
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorGenerating),ex.Message),
                ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_GeneratingError)), MessageBoxButtons.Ok, MessageBoxIcons.Error);
            await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
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
                Title = ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_SelectAFile)),
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_FileFilterDescriptionJson))) { Patterns = ["*.json"] },
                    new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_AllFiles))) { Patterns = ["*"] }
                ]
            });

            if (files.Count == 1)
            {
                try
                {
                    await LoadSettingsAsync(files[0].Path.LocalPath);
                }
                catch (Exception ex)
                {
                    await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorLoadingPreset),ex.Message), 
                        ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_LoadError)), MessageBoxButtons.Ok, MessageBoxIcons.Error);
                }
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorLoading)," JSON",ex.Message), ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_Error)), MessageBoxButtons.Ok,  MessageBoxIcons.Error);
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
                Title = ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_SavePreset)),
                DefaultExtension =  ".json",
                FileTypeChoices = 
                [
                    new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_FileFilterDescriptionJson))) { Patterns = ["*.json"] },
                    new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_AllFiles))) { Patterns = ["*"] }
                ]
            });

            if (file is not null)
            {
                try
                {
                    await SaveSettingsAsync(file.Path.LocalPath);
                    await MessageDialog.ShowAsync(this, ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_PresetSavedSuccessfully)), 
                        ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_SaveComplete)), MessageBoxButtons.Ok, MessageBoxIcons.Information);
                }
                catch (Exception ex)
                {
                    await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorSavingPreset),ex.Message), 
                        ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorSaving),null,null), MessageBoxButtons.Ok, MessageBoxIcons.Error);
                }
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorSaving),"JSON",ex.Message), ResourceHelper.GetString(nameof(Assets.Resources.MainWindow_Error)), MessageBoxButtons.Ok,  MessageBoxIcons.Error);
            await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
        }
    }

    private bool AllSettingsInJson(string json)
    {
        JsonNode? obj = JsonNode.Parse(json);
        if (obj is null) return false;
        
        var jsonProps = obj.AsObject().Select(p => p.Key).ToHashSet();
        var classProps = typeof(SettingsViewModel).GetProperties().Select(p => p.Name).ToHashSet();

        return classProps.All(jsonProps.Contains);
    }

    private async Task LoadSettingsAsync(string pathToJson)
    {
        try
        {
            string json;
            using (StreamReader r = new StreamReader(pathToJson))
            {
                json = await r.ReadToEndAsync();
                var newSettingsData = JsonSerializer.DeserializeThrowOnNull(json, JsonSourceGenerationContext.Default.SettingsViewModel);
                RandomizerLogic.Settings = newSettingsData;
                DataContext = RandomizerLogic.Settings;
            }
            
            if (!AllSettingsInJson(json))
            {
                await SaveSettingsAsync(pathToJson);
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorLoading),null,ex.Message),
                ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorLoading),null,null), MessageBoxButtons.Ok, MessageBoxIcons.Error);
            await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
        }
    }

    private async Task SaveSettingsAsync(string pathToJson)
    {
        try
        {
            await using StreamWriter r = new StreamWriter(pathToJson);
            string json = JsonSerializer.Serialize(RandomizerLogic.Settings, JsonSourceGenerationContextSerializationFactory.LazyJsonSourceGenerationContext.Value.SettingsViewModel);
            await r.WriteAsync(json);
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorSaving),null,ex.Message),
                ResourceHelper.GetStringFormatted(nameof(Assets.Resources.MainWindow_ErrorSaving),null,null), MessageBoxButtons.Ok, MessageBoxIcons.Error);
            await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
        }
    }
}


public class SettingsViewModel : ObservableObject
{
    public int Seed { get; set; } = -1;
    
    public bool RandomizeEnemies { get; set; } = true;
    
    public bool RandomizeEncounterSizes { get; set; } 
    public bool ChangeSizeOfNonRandomizedEncounters { get; set; } 
    public bool EncounterSizeOne { get; set; } 
    public bool EncounterSizeTwo { get; set; } 
    public bool EncounterSizeThree { get; set; } 
    public bool NoSimonP2BeforeLune { get; set; } = true;
    public bool RandomizeMerchantFights { get; set; } = true;
    public bool EnableEnemyOnslaught { get; set; } 
    public int EnemyOnslaughtAdditionalEnemies { get; set; } = 1;
    public int EnemyOnslaughtEnemyCap { get; set; } = 4;
    public bool IncludeCutContentEnemies { get; set; } = true;

    public bool RandomizeAddedEnemies { get; set; } 
    public bool EnsureBossesInBossEncounters { get; set; } 
    public bool ReduceBossRepetition { get; set; } 
    // public bool TieDropsToEncounters { get; set; }  
    

    public bool RandomizeItems { get; set; } = true;
    public bool ChangeSizesOfNonRandomizedChecks { get; set; } 
    
    public bool ReduceKeyItemRepetition { get; set; } = true;
    public bool ReduceGearRepetition { get; set; } = true;
    public bool RandomizeEsquieRocks { get; set; } 
    public bool LimitEsquieRandomization { get; set; } = true;

    public bool ChangeMerchantInventorySize { get; set; } 
    public int MerchantInventorySizeMax { get; set; } = 15;
    public int MerchantInventorySizeMin { get; set; } = 1;
    
    public bool ChangeItemQuantity { get; set; } 
    public int ItemQuantityMax { get; set; } = 10;
    public int ItemQuantityMin { get; set; } = 1;
    
    public bool ChangeMerchantInventoryLocked { get; set; } 
    public int MerchantInventoryLockedChancePercent { get; set; } = 10;
    
    public bool ChangeNumberOfLootDrops { get; set; } 
    public int LootDropsNumberMax { get; set; } = 3;
    public int LootDropsNumberMin { get; set; }

    public bool ChangeNumberOfTowerRewards { get; set; } 
    public int TowerRewardsNumberMax { get; set; } = 3;
    public int TowerRewardsNumberMin { get; set; } = 1;
    
    public bool ChangeNumberOfChestContents { get; set; } 
    public int ChestContentsNumberMax { get; set; } = 2;
    public int ChestContentsNumberMin { get; set; } = 1;
    
    public bool ChangeNumberOfActionRewards { get; set; } 
    public int ActionRewardsNumberMax { get; set; } = 3;
    public int ActionRewardsNumberMin { get; set; } = 1;
    
    public bool MakeEveryItemVisible { get; set; } = true;
    
    public bool EnsurePaintedPowerFromPaintress { get; set; } = true;
    public bool IncludeGearInPrologue { get; set; } 
    public bool RandomizeStartingWeapons { get; set; } 
    public bool RandomizeStartingCosmetics { get; set; } 
    public bool RandomizeGestralBeachRewards { get; set; } = true;
    public bool RandomizeMonocoFeet { get; set; } = true;
    public bool IncludeCutContentItems { get; set; } = true;
    
    public bool RandomizeSkills { get; set; } = true;
    public bool ReduceSkillRepetition { get; set; } = true;
    public bool IncludeCutContentSkills { get; set; } 
    public bool UnlockGustaveSkills { get; set; } 
    public bool GuaranteeGustaveOvercharge { get; set; } = true;
    public bool RandomizeSkillUnlockCosts { get; set; } 
    public bool RandomizeTreeEdges { get; set; } = true;
    //TODO: Add dummy edge structs to Monoco's asset
    public bool GiveMonocoTreeEdges { get; set; } 
    public int MinTreeEdges { get; set; } = 2;
    public int MaxTreeEdges { get; set; } = 4;
    public bool FullyRandomEdges { get; set; } 
    public int RandomEdgeChancePercent { get; set; } = 60;
    public bool MakeSkillsIntoItems { get; set; } 
    
    public bool RandomizeLocations { get; set; } = true;
    public bool ReduceLocationRepetition { get; set; } 
    public bool RescaleLocations { get; set; } = true;
    public bool RescaleCharacters { get; set; } = true;
    
    public bool RandomizeStartingLocation { get; set; }
    public bool RandomizeManorDoors { get; set; } = true;
    public bool RandomizeWorkshopEntries { get; set; } = true;
    public bool RandomizeCutsceneTeleports { get; set; } = true;
    public bool RandomizeGestralBeachPortals { get; set; } = true;

    
    public bool RandomizeCharacters { get; set; } 
    
    public bool ScaleOptionalAreas { get; set; } = true;
    public int ScaleModifierPercentage { get; set; } = 100;
    
    
    
    [JsonIgnore]
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
