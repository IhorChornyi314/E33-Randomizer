using System.Diagnostics.CodeAnalysis;
using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit.TextMate;
using E33Randomizer.ObjectDatum;
using E33Randomizer.UIControls;
using TextMateSharp.Grammars;

namespace E33Randomizer.CustomPlacements;
    public partial class CustomPlacementWindow : Window
    {
        private CustomPlacementWindowViewModel CustomPlacement => (DataContext as CustomPlacementWindowViewModel)!;

        private TextMate.Installation _textMateInstallation;
        private static readonly RegistryOptions TextMateOptions = new RegistryOptions(ThemeName.DarkPlus);
        private static readonly string JsonLanguageId = TextMateOptions.GetScopeByLanguageId(TextMateOptions.GetLanguageByExtension(".json").Id);
        
        // Allowing for Design-Time viewer
        public CustomPlacementWindow()
        {
            DataContext = new CustomEnemyPlacement()
            {
                AllObjects = new List<ObjectData>()
                {
                    new ItemData(){ CodeName = "Short", CustomName = "SomeShortName"}
                },
                PresetFiles = new ObservableCollectionWithChildListener<MenuItemViewModel>
                {
                    {"Split categories (default)", "Data/Presets/enemies/default.json"},
                    {"Total randomness", "Data/Presets/enemies/total_random.json"},
                    {"10% of regular enemies are bosses", "Data/Presets/enemies/10_percent.json"},
                    {"Make every enemy a boss", "Data/Presets/enemies/everyone_is_a_boss.json"},
                    {"Custom preset 1", "Data/Presets/enemies/custom_1.json"},
                    {"Custom preset 2", "Data/Presets/enemies/custom_2.json"},
                    {"Oops All...", "OopsAll"}
                }
            };
            InitializeComponent();
            SetupTextMateTheme();
            SetupAutoCompleteBehaviors();
        }

      

        public CustomPlacementWindow(CustomPlacementWindowViewModel customPlacement)
        {
            DataContext = customPlacement;
            InitializeComponent();
            SetupTextMateTheme();
            SetupAutoCompleteBehaviors();
        }

        [MemberNotNull(nameof(_textMateInstallation))]
        private void SetupTextMateTheme()
        {
            _textMateInstallation = JsonText.InstallTextMate(TextMateOptions);
            _textMateInstallation.SetTheme(TextMateOptions.LoadTheme(ThemeName.DarkPlus));
            SetSyntaxHighlighting(CustomPlacement.JsonSyntaxHighlighting);
        }

        private void SetSyntaxHighlighting(bool isEnabled)
        {
            _textMateInstallation.SetGrammar(isEnabled && JsonExpander.IsExpanded ? JsonLanguageId : null);
        }

        private void SetupAutoCompleteBehaviors()
        {
            NotRandomizedObjectsSelectionComboBox.AddAutoDropDownOnFocusAndClickHandler();
            ExcludedObjectsSelectionComboBox.AddAutoDropDownOnFocusAndClickHandler();
        }
        
        private void NotRandomizedObjectsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = NotRandomizedObjectsSelectionComboBox.SelectedItem?.ToString();
            if (selectedItem == null) return;

            NotRandomizedObjectsSelectionComboBox.SelectedItem = null;
            
            CustomPlacement.NotRandomized.Add(selectedItem);
            
            // Must defer till later since this is updating the same object that triggered the update.
            Dispatcher.UIThread.Post(() =>
            {
                CustomPlacement.NotRandomizedOptions.Remove(selectedItem);
            });
        }


        private void NotExcludedObjectsComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ExcludedObjectsSelectionComboBox.SelectedItem?.ToString();
            if (selectedItem == null) return;

            ExcludedObjectsSelectionComboBox.SelectedItem = null;
            
            CustomPlacement.Excluded.Add(selectedItem);
            
            // Must defer till later since this is updating the same object that triggered the update.
            Dispatcher.UIThread.Post(() =>
            {
                CustomPlacement.ExcludedOptions.Remove(selectedItem);
            });
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
                    Title = ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_LoadCustomPreset)),
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_JSONFilesJson))) { Patterns = ["*.json"] },
                        new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_AllFiles))) { Patterns = ["*"] }
                    ]
                });

                if (files.Count == 1)
                {
                    try
                    {
                        CustomPlacement.LoadFromJson(files[0].Path.LocalPath);
                    }
                    catch (Exception ex)
                    {
                        await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.CustomPlacement_ErrorLoadingPreset),ex.Message), 
                            ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_LoadError)), MessageBoxButtons.Ok, MessageBoxIcons.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.CustomPlacement_ErrorLoadingPreset),ex.Message), ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_LoadError)), MessageBoxButtons.Ok,  MessageBoxIcons.Error);
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
                    Title = ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_SaveCustomPreset)),
                    DefaultExtension =  ".json",
                    FileTypeChoices = 
                    [
                        new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_JSONFilesJson))) { Patterns = ["*.json"] },
                        new FilePickerFileType(ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_AllFiles))) { Patterns = ["*"] }
                    ]
                });

                if (file is not null)
                {
                    try
                    {
                        CustomPlacement.SaveToJson(file.Path.LocalPath);
                        await MessageDialog.ShowAsync(this, ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_PresetSavedSuccessfully)), 
                            ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_SaveComplete)), MessageBoxButtons.Ok, MessageBoxIcons.Information);
                    }
                    catch (Exception ex)
                    {
                        await MessageDialog.ShowAsync
                        (this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.CustomPlacement_ErrorSavingPreset),ex.Message), 
                            ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_SaveError)), MessageBoxButtons.Ok, MessageBoxIcons.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                await MessageDialog.ShowAsync(this, ResourceHelper.GetStringFormatted(nameof(Assets.Resources.CustomPlacement_ErrorSavingJSON),ex.Message), ResourceHelper.GetString(nameof(Assets.Resources.CustomPlacement_Error)), MessageBoxButtons.Ok,  MessageBoxIcons.Error);
                await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                SetSyntaxHighlighting(toggleButton.IsChecked ?? false);
            }
        }
    }