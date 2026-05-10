using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace E33Randomizer;
    public partial class CustomPlacementWindow : Window
    {
        public string? SelectedObjectForCustomPlacement = null;
        private CustomPlacement CustomPlacement;

        // Allowing for Design-Time viewer
        public CustomPlacementWindow()
        {
            CustomPlacement = new CustomItemPlacement()
            {
                AllObjects = new List<ObjectData>()
                {
                    new ItemData(){ CodeName = "Short", CustomName = "SomeShortName"}
                },
                PresetFiles = new Dictionary<string, string>()
                {
                    {"Split categories (default)", "Data/Presets/enemies/default.json"},
                    {"Total randomness", "Data/Presets/enemies/total_random.json"},
                    {"10% of regular enemies are bosses", "Data/Presets/enemies/10_percent.json"},
                    {"Make every enemy a boss", "Data/Presets/enemies/everyone_is_a_boss.json"},
                    {"Custom preset 1", "Data/Presets/enemies/custom_1.json"},
                    {"Custom preset 2", "Data/Presets/enemies/custom_2.json"},
                }
            };
            DataContext = new CustomPlacementWindowViewModel(CustomPlacement, this);
            LoadCustomPlacementRows(SelectedObjectForCustomPlacement);
            InitializeComponent();
            PopulateObjectDropdowns();
            // InitPresetButtons();
            Update();
        }
        
        public CustomPlacementWindow(CustomPlacement customPlacement)
        {
            CustomPlacement = customPlacement;
            DataContext = new CustomPlacementWindowViewModel(CustomPlacement, this);
            InitializeComponent();
            PopulateObjectDropdowns();
            // InitPresetButtons();
            Update();
        }

        // private void InitPresetButtons()
        // {
        //     int i = 0;
        //     foreach (var presetFile in CustomPlacement.PresetFiles)
        //     {
        //         var presetButton = this.FindNameScope()?.Find<MenuItem>($"PresetButton{i + 1}");
        //         presetButton.Header = presetFile.Key;
        //         presetButton.Tag = presetFile.Value;
        //         i++;
        //     }
        // }

        private void Update()
        {
            UpdateExcludedListBox();
            UpdateNotRandomizedListBox();
            UpdateJsonTextBox();
        }

        private void UpdateJsonTextBox()
        {
            var presetData = new CustomPlacementPreset(CustomPlacement.NotRandomized, CustomPlacement.Excluded, CustomPlacement.CustomPlacementRules, CustomPlacement.FrequencyAdjustments);
            string json = JsonSerializer.Serialize(presetData, JsonSourceGenerationContextSerializationFactory.LazyJsonSourceGenerationContext.Value.CustomPlacementPreset);
            // PresetJsonTextBox.Text = json;
        }
        
        private void UpdateExcludedListBox()
        {
            ExcludedObjectsListBox.Items.Clear();
            foreach (var excludedOption in CustomPlacement.Excluded)
            {
                ExcludedObjectsListBox.Items.Add(excludedOption);
            }
        }

        private void UpdateNotRandomizedListBox()
        {
            NotRandomizedObjectsListBox.Items.Clear();
            foreach (var notRandomizedOption in CustomPlacement.NotRandomized)
            {
                NotRandomizedObjectsListBox.Items.Add(notRandomizedOption);
            }
        }
        
        private void PopulateObjectComboBox(ComboBox comboBox)
        {
            foreach (string objectPlainName in CustomPlacement.PlainNamesList)
            {
                comboBox.Items.Add(new ComboBoxItem { Content = objectPlainName });
            }
        }
        
        private void PopulateObjectDropdowns()
        {
            PopulateObjectComboBox(NotRandomizedObjectsSelectionComboBox);
            PopulateObjectComboBox(ExcludedObjectsComboBox);
            PopulateObjectComboBox(OopsAllObjectComboBox);
            
            foreach (string objectPlainName  in CustomPlacement.PlainNamesList)
            {
                CustomPlacementObjectListBox.Items.Add(new ComboBoxItem { Content = objectPlainName });
            }
        }

        private void NotRandomizedObjectsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotRandomizedObjectsSelectionComboBox.SelectedItem != null)
            {
                AddNotRandomizedObject();
            }
        }

        private void AddNotRandomizedObject()
        {
            if (NotRandomizedObjectsSelectionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string objectName = selectedItem.Content.ToString();
                
                if (!CustomPlacement.NotRandomized.Contains(objectName))
                {
                    CustomPlacement.AddNotRandomized(objectName);
                    UpdateJsonTextBox();
                }
                
                NotRandomizedObjectsSelectionComboBox.SelectedItem = null;
                UpdateNotRandomizedListBox();
            }
        }

        private void NotRandomizedObjectsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveNotRandomizedObjectButton.IsEnabled = NotRandomizedObjectsListBox.SelectedItem != null;
        }

        private void RemoveNotRandomizedObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (NotRandomizedObjectsListBox.SelectedItem != null)
            {
                string selectedObject = NotRandomizedObjectsListBox.SelectedItem.ToString();

                CustomPlacement.RemoveNotRandomized(selectedObject);
                
                UpdateNotRandomizedListBox();
                UpdateJsonTextBox();
                RemoveNotRandomizedObjectButton.IsEnabled = false;
            }
        }
        
        private void ExcludedObjectsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExcludedObjectsComboBox.SelectedItem != null)
            {
                AddExcludedObject();
            }
        }

        private void AddExcludedObject()
        {
            if (ExcludedObjectsComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string objectName = selectedItem.Content.ToString();
                
                if (!CustomPlacement.Excluded.Contains(objectName))
                {
                    CustomPlacement.AddExcluded(objectName);
                    UpdateExcludedListBox();
                    UpdateJsonTextBox();
                }
                else
                {
                    MessageDialog.Show(this, $"{objectName} is already in the selected Objects list.", 
                                    "Duplicate Object", nameof(DialogBoxButton.OK), MessageBoxIcons.Information);
                }
                
                ExcludedObjectsComboBox.SelectedItem = null;
            }
        }

        private void ExcludedObjectsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveExcludedObjectButton.IsEnabled = ExcludedObjectsListBox.SelectedItem != null;
        }

        private void RemoveExcludedObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExcludedObjectsListBox.SelectedItem != null)
            {
                string selectedObject = ExcludedObjectsListBox.SelectedItem.ToString();
                CustomPlacement.RemoveExcluded(selectedObject);
                UpdateExcludedListBox();
                UpdateJsonTextBox();
                RemoveExcludedObjectButton.IsEnabled = false;
            }
        }

        private void OopsAllObjectComboBox_Change(object sender, SelectionChangedEventArgs e)
        {
           
            string objectCodeName = (e.AddedItems[0] as ComboBoxItem).Content.ToString();
            CustomPlacement.ApplyOopsAll(objectCodeName);
            
            LoadCustomPlacementRows(SelectedObjectForCustomPlacement);
            Update();
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
                        LoadCustomPlacementRows(SelectedObjectForCustomPlacement);
                        Update();
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
                await MessageDialog.ShowAsync(this, $"Error Loading preset: {ex.Message}", "Load Error", nameof(DialogBoxButton.OK),  MessageBoxIcons.Error);
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
                            "Save Complete", nameof(DialogBoxButton.OK), MessageBoxIcons.Information);
                    }
                    catch (Exception ex)
                    {
                        await MessageDialog.ShowAsync
                        (this, $"Error saving preset: {ex.Message}", 
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

        private void CustomPlacementObjectListBox_SelectionChanged(object sender, FocusChangedEventArgs e)
        {
            var item = (ListBoxItem)sender;
            if (item.Content is string selectedObject)
            {
                SelectedObjectForCustomPlacement = selectedObject;
                SelectedObjectDisplay.Text = $"Selected: {selectedObject}";
                AddCustomPlacementRowButton.IsEnabled = true;
                
                LoadCustomPlacementRows(selectedObject);
            }
            else
            {
                SelectedObjectForCustomPlacement = null;
                SelectedObjectDisplay.Text = "No object selected";
                AddCustomPlacementRowButton.IsEnabled = false;
                CustomPlacementRowsContainer.Children.Clear();
            }
        }

        private void AddCustomPlacementRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedObjectForCustomPlacement != null)
            {
                var row = CreateCustomPlacementRow("");
                CustomPlacementRowsContainer.Children.Add(row);
            }
        }

        private StackPanel CreateCustomPlacementRow(string objectName)
        {
            StackPanel row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 5)
            };


            ComboBox objectNameCombo = new ComboBox
            {
                Width = 150,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0)
            };
            PopulateObjectComboBox(objectNameCombo);
            if (objectName != "")
            {
                objectNameCombo.SelectedIndex = CustomPlacement.PlainNamesList.IndexOf(objectName);
            }
            
            Button removeButton = new Button
            {
                Content = "-",
                Width = 30,
                Height = 30,
                Margin = new Thickness(0, 0, 5, 0),
                Background = Brushes.Red,
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold
            };
            removeButton.Click += (_, _) => RemoveCustomPlacementRow((string)(objectNameCombo.SelectedItem as ComboBoxItem)?.Content, row);

            float frequency = 1;

            if (SelectedObjectForCustomPlacement != "" && CustomPlacement.CustomPlacementRules.ContainsKey(SelectedObjectForCustomPlacement) &&
                CustomPlacement.CustomPlacementRules[SelectedObjectForCustomPlacement].ContainsKey(objectName))
            {
                frequency = CustomPlacement.CustomPlacementRules[SelectedObjectForCustomPlacement][objectName];
            }
            
            Slider frequencySlider = new Slider
            {
                Width = 100,
                Height = 30,
                Minimum = 0,
                Maximum = 100,
                Value = frequency * 100,
                Margin = new Thickness(0, 0, 10, 0)
            };

            TextBox frequencyTextBox = new TextBox
            {
                Width = 60,
                Height = 30,
                Text = (frequency * 100).ToString("F1"),
                VerticalContentAlignment = VerticalAlignment.Center
            };
    
            
            objectNameCombo.SelectionChanged += (_, _) =>
            {
                CustomPlacement.SetCustomPlacement(SelectedObjectForCustomPlacement,
                    (string)(objectNameCombo.SelectedItem as ComboBoxItem).Content, (float)frequencySlider.Value / 100); 
                UpdateJsonTextBox();
            };
            
            frequencySlider.ValueChanged += (_, e) => {
                if (!frequencyTextBox.IsFocused)
                {
                    frequencyTextBox.Text = e.NewValue.ToString("F1");
                }
                if (objectNameCombo.SelectedItem != null)
                {
                    CustomPlacement.SetCustomPlacement(SelectedObjectForCustomPlacement, (string)(objectNameCombo.SelectedItem as ComboBoxItem).Content, (float)e.NewValue / 100);
                    UpdateJsonTextBox();
                }
            };

            frequencyTextBox.TextInput += (_, e) =>
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            };

            frequencyTextBox.TextChanged += (_, _) => {
                frequencyTextBox.Text = frequencyTextBox.Text.Replace(" ", "");

                if (double.TryParse(frequencyTextBox.Text, out double value) && value >= 0)
                {
                    frequencySlider.Value = value;
                    if (objectNameCombo.SelectedItem != null)
                    {
                        CustomPlacement.SetCustomPlacement(SelectedObjectForCustomPlacement,
                            (string)(objectNameCombo.SelectedItem as ComboBoxItem).Content, (float)value / 100);
                        UpdateJsonTextBox();
                    }
                }
            };

            Label percentLabel = new Label
            {
                Content = "%"
            };

            row.Children.Add(removeButton);
            row.Children.Add(objectNameCombo);
            row.Children.Add(frequencySlider);
            row.Children.Add(frequencyTextBox);
            row.Children.Add(percentLabel);

            return row;
        }

        private void RemoveCustomPlacementRow(string? objectName, StackPanel row)
        {
            CustomPlacementRowsContainer.Children.Remove(row);
            if (objectName != null)
            {
                CustomPlacement.RemoveCustomPlacement(SelectedObjectForCustomPlacement, objectName);
                UpdateJsonTextBox();
            }
        }

        private void LoadCustomPlacementRows(string? objectName)
        {
            if (objectName == null)
            {
                return;
            }
            CustomPlacementRowsContainer.Children.Clear();
            if (!CustomPlacement.CustomPlacementRules.TryGetValue(objectName, out var customPlacements))
            {
                return;
            }
            foreach (var pair in customPlacements)
            {
                var row = CreateCustomPlacementRow(pair.Key);
                CustomPlacementRowsContainer.Children.Add(row);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, WindowClosingEventArgs e)
        {
            
        }
    }

public sealed partial class CustomPlacementWindowViewModel : ObservableObject
{
    private readonly Window _window;
    public CustomPlacement CustomPlacement { get; }

    // TODO Remove window and fix error handling.
    public CustomPlacementWindowViewModel(CustomPlacement customPlacement, Window window)
    {
        _window = window;
        CustomPlacement = customPlacement;
        FrequencyAdjustments = new ();
        FrequencyAdjustments.CollectionChanged += FrequencyAdjustmentsOnCollectionChanged;
        
        PresetFiles = new();
        CustomCategories = new();
        LoadPresetsFromModel();
        UpdateFromModel();
    }

    private void FrequencyAdjustmentsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems?[0] is not StringFloatKeyValuePairViewModel item) return;
                if (item.Key == "SelectOne") return;
                CustomPlacement.FrequencyAdjustments.TryAdd(item.Key, item.Value);
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems?[0] is not StringFloatKeyValuePairViewModel itemRemoved) return;
                if (itemRemoved.Key == "SelectOne") return;
                CustomPlacement.FrequencyAdjustments.Remove(itemRemoved.Key);
                break;
        }
        
        UpdateJsonTextBox();
    }

    private void UpdateFromModel()
    {
        LoadCategoriesFromModel();
        LoadFrequenciesFromModel();
    }

    private void LoadFrequenciesFromModel()
    {
        FrequencyAdjustments.Clear();
        foreach (var frequency in CustomPlacement.FrequencyAdjustments)
        {
            FrequencyAdjustments.Add(new (frequency.Key, frequency.Value * 100));
        }
    }

    private void LoadCategoriesFromModel()
    {
        CustomCategories.Clear();
        foreach (var category in CustomPlacement.CustomCategories)
        {
            CustomCategories.Add(category);
        }
    }

    private void LoadPresetsFromModel()
    {
        PresetFiles.Clear();
        foreach (var preset in CustomPlacement.PresetFiles)
        {
            PresetFiles.Add(new MenuItemViewModel(preset.Key, preset.Value, LoadPreset));
        }
    }
        
    [ObservableProperty]
    public partial ObservableCollection<StringFloatKeyValuePairViewModel> FrequencyAdjustments { get; set; }

    [ObservableProperty]
    public partial string? OopsAllObjectsSelection { get; set; } = null;

    [ObservableProperty] 
    public partial string Json { get; set; } = string.Empty;

    [ObservableProperty] 
    public partial bool SelectedPresetIsOops { get; set; } = false;

    [ObservableProperty]
    public partial ObservableCollection<MenuItemViewModel> PresetFiles { get; set; }
    
    [ObservableProperty]
    public partial ObservableCollection<string> CustomCategories { get; set; }

    public string? SelectedObjectForCustomPlacement = null;
    
    
    partial void OnOopsAllObjectsSelectionChanged(string? value)
    {
        if (value is null) return;
        
        CustomPlacement.ApplyOopsAll(value);
    }
    
    private async Task LoadPreset(string presetFile)
    {
        SelectedPresetIsOops = false;
        try
        {
            CustomPlacement.LoadFromJson(presetFile.Replace("Data", RandomizerLogic.DataDirectory));
            UpdateFromModel();
            LoadCustomPlacementRows(SelectedObjectForCustomPlacement);
            UpdateJsonTextBox();
        }
        catch (Exception ex)
        {
            //TODO FIX
            await MessageDialog.ShowAsync
            (_window, $"Error loading preset: {ex.Message}; Reverting to Default.",
                "Load Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
            CustomPlacement.LoadDefaultPreset();
            UpdateJsonTextBox();
        }
    }

    [RelayCommand]
    private void AddFrequencyRow()
    {
        FrequencyAdjustments.Add(new("SelectOne", 0));
    }

    [RelayCommand]
    private void OopsAllButton()
    {
        SelectedPresetIsOops = true;
    }

    [RelayCommand]
    private void RemoveFrequencyRow(StringFloatKeyValuePairViewModel model)
    {
        if (string.IsNullOrEmpty(model.Key)) return;
        
        FrequencyAdjustments.Remove(model);
        CustomPlacement.FrequencyAdjustments.Remove(model.Key);
    }

    //TODO FIX
    private void LoadCustomPlacementRows(string? objectName)
    {
        if (objectName == null)
        {
            return;
        }
        //CustomPlacementRowsContainer.Children.Clear();
        if (!CustomPlacement.CustomPlacementRules.TryGetValue(objectName, out var customPlacements))
        {
            return;
        }
        foreach (var pair in customPlacements)
        {
            //var row = CreateCustomPlacementRow(pair.Key);
            //CustomPlacementRowsContainer.Children.Add(row);
        }
    }
    
    private void UpdateJsonTextBox()
    {
        var presetData = new CustomPlacementPreset(CustomPlacement.NotRandomized, CustomPlacement.Excluded, CustomPlacement.CustomPlacementRules, CustomPlacement.FrequencyAdjustments);
        string json = JsonSerializer.Serialize(presetData, JsonSourceGenerationContextSerializationFactory.LazyJsonSourceGenerationContext.Value.CustomPlacementPreset);
        Json = json;
    }

    
}

public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string FilePath { get; set; }

    [ObservableProperty]
    public partial Func<string, Task> LoadPresetAction { get; set; }
    
    [RelayCommand]
    public async Task LoadPreset()
    {
        await LoadPresetAction(FilePath);
    }

    public MenuItemViewModel(string name, string filePath, Func<string, Task> loadPresetAction)
    {
        Name = name;
        FilePath = filePath;
        LoadPresetAction = loadPresetAction;
    }
}

public partial class StringFloatKeyValuePairViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Key { get; set; }

    [ObservableProperty]
    public partial float Value { get; set; }

    public StringFloatKeyValuePairViewModel(string key, float value)
    {
        Key = key;
        Value = value;
    }

    partial void OnKeyChanged(string value)
    {
        Console.WriteLine(value);
    }
}