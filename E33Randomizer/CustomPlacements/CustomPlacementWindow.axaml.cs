using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using E33Randomizer.ObjectDatum;
using E33Randomizer.UIControls;

namespace E33Randomizer.CustomPlacements;
    public partial class CustomPlacementWindow : Window
    {
        private CustomPlacementWindowViewModel CustomPlacement => (DataContext as CustomPlacementWindowViewModel)!;

        // Allowing for Design-Time viewer
        public CustomPlacementWindow()
        {
            DataContext = new CustomItemPlacement()
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
                }
            };
            InitializeComponent();
            SetupAutoCompleteBehaviors();
        }

      

        public CustomPlacementWindow(CustomPlacementWindowViewModel customPlacement)
        {
            DataContext = customPlacement;
            InitializeComponent();
            SetupAutoCompleteBehaviors();
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
                    Title = "Load Custom Preset",
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
                        CustomPlacement.LoadFromJson(files[0].Path.LocalPath);
                        //LoadCustomPlacementRows(_selectedObjectForCustomPlacement);
                    }
                    catch (Exception ex)
                    {
                        await MessageDialog.ShowAsync(this, $"Error loading preset: {ex.Message}", 
                            "Load Error", MessageBoxButtons.Ok, MessageBoxIcons.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                await MessageDialog.ShowAsync(this, $"Error Loading preset: {ex.Message}", "Load Error", MessageBoxButtons.Ok,  MessageBoxIcons.Error);
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
                    Title = "Save Custom Preset",
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
                        CustomPlacement.SaveToJson(file.Path.LocalPath);
                        await MessageDialog.ShowAsync(this, "Preset saved successfully!", 
                            "Save Complete", MessageBoxButtons.Ok, MessageBoxIcons.Information);
                    }
                    catch (Exception ex)
                    {
                        await MessageDialog.ShowAsync
                        (this, $"Error saving preset: {ex.Message}", 
                            "Save Error", MessageBoxButtons.Ok, MessageBoxIcons.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                await MessageDialog.ShowAsync(this, $"Error Saving JSON: {ex.Message}", "Error", MessageBoxButtons.Ok,  MessageBoxIcons.Error);
                await File.WriteAllTextAsync(Program.CrashLogFileName, ex.ToString(), Encoding.UTF8);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OopsAllObjectComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            CustomPlacement.ApplyOopsAll();
        }
    }