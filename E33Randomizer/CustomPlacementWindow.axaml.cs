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


namespace E33Randomizer
{
    public partial class CustomPlacementWindow : Window
    {
        public string SelectedObjectForCustomPlacement = null;
        private CustomPlacement CustomPlacement;

        public CustomPlacementWindow(CustomPlacement customPlacement)
        {
            CustomPlacement = customPlacement;
            InitializeComponent();
            PopulateObjectDropdowns();
            InitPresetButtons();
            Update();
        }

        private void InitPresetButtons()
        {
            int i = 0;
            foreach (var presetFile in CustomPlacement.PresetFiles)
            {
                var presetButton = this.FindNameScope()?.Find<Button>($"PresetButton{i + 1}");
                presetButton.Content = presetFile.Key;
                presetButton.Tag = presetFile.Value;
                i++;
            }
        }

        private void Update()
        {
            UpdateExcludedListBox();
            UpdateNotRandomizedListBox();
            UpdateFrequencies();
            UpdateJsonTextBox();
        }

        private void UpdateJsonTextBox()
        {
            var presetData = new CustomPlacementPreset(CustomPlacement.NotRandomized, CustomPlacement.Excluded, CustomPlacement.CustomPlacementRules, CustomPlacement.FrequencyAdjustments);
            string json = JsonSerializer.Serialize(presetData, JsonSourceGenerationContextSerializationFactory.LazyJsonSourceGenerationContext.Value.CustomPlacementPreset);
            PresetJsonTextBox.Text = json;
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

        private void UpdateFrequencies()
        {
            FrequencyRowsContainer.Children.Clear();
            foreach (var frequencyAdjustment in CustomPlacement.FrequencyAdjustments)
            {
                var newRow = CreateFrequencyRow(frequencyAdjustment.Key);
                FrequencyRowsContainer.Children.Add(newRow);
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

        private void OopsAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (OopsAllObjectComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string objectCodeName = selectedItem.Content.ToString();
                CustomPlacement.ApplyOopsAll(objectCodeName);
                
                LoadCustomPlacementRows(SelectedObjectForCustomPlacement);
                Update();
            }
            else
            {
                MessageDialog.Show(this, "Please select an object from the dropdown first.", 
                               "No Object Selected", nameof(DialogBoxButton.OK), MessageBoxIcons.Warning);
            }
        }

        private async void PresetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button { Tag: string presetFile })
                {
                    try
                    {
                        CustomPlacement.LoadFromJson(presetFile.Replace("Data", RandomizerLogic.DataDirectory));
                        LoadCustomPlacementRows(SelectedObjectForCustomPlacement);
                        Update();
                    }
                    catch (Exception ex)
                    {
                        await MessageDialog.ShowAsync
                        (this, $"Error loading preset: {ex.Message}; Reverting to Default.",
                            "Load Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
                        CustomPlacement.LoadDefaultPreset();
                        UpdateJsonTextBox();
                    }
                }
            }
            catch(Exception ex)
            {
                await MessageDialog.ShowAsync(this, $"Error loading preset: {ex.Message}; Reverting to Default.", "Load Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
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

        private void AddFrequencyRowButton_Click(object sender, RoutedEventArgs e)
        {
            var newRow = CreateFrequencyRow("");
            FrequencyRowsContainer.Children.Add(newRow);
        }

        private StackPanel CreateFrequencyRow(string objectName)
        {
            StackPanel row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 5)
            };


            ComboBox objectCombo = new ComboBox
            {
                Width = 150,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            PopulateObjectComboBox(objectCombo);
            
            Button removeButton = new Button
            {
                Content = "-",
                Width = 30,
                Height = 30,
                Margin = new Thickness(0, 0, 5, 0),
                Background =Brushes.Red,
                Foreground =Brushes.White,
                FontWeight = FontWeight.Bold
            };
            removeButton.Click += (_, _) =>
            {
                var objectName = (string)(objectCombo.SelectedItem as ComboBoxItem)?.Content;
                if (objectName != null)
                {
                    CustomPlacement.FrequencyAdjustments.Remove(objectName);
                }
                RemoveFrequencyRow(row);
                UpdateJsonTextBox();
            };

            if (objectName != "")
            {
                objectCombo.SelectedIndex = CustomPlacement.PlainNamesList.IndexOf(objectName);
            }

            Slider frequencySlider = new Slider
            {
                Width = 100,
                Height = 30,
                Minimum = 0,
                Maximum = 100,
                Value = objectName == "" ? 100 : CustomPlacement.FrequencyAdjustments[objectName] * 100,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBox frequencyTextBox = new TextBox
            {
                Width = 60,
                Height = 30,
                Text = objectName == "" ? "100" : (CustomPlacement.FrequencyAdjustments[objectName] * 100).ToString("F1"),
                VerticalContentAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Label percentLabel = new Label
            {
                Content = "%",
                VerticalAlignment = VerticalAlignment.Center
            };

            frequencySlider.ValueChanged += (_, e) => {
                if (!frequencyTextBox.IsFocused)
                {
                    frequencyTextBox.Text = e.NewValue.ToString("F1");
                    if (objectCombo.SelectedItem != null)
                    {
                        CustomPlacement.FrequencyAdjustments[
                            (string)(objectCombo.SelectedItem as ComboBoxItem).Content] = (float)e.NewValue / 100;
                        UpdateJsonTextBox();
                    }
                }
            };

            frequencyTextBox.TextChanged += (_, e) =>
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch((e.Source as TextBox)!.Text!);
                UpdateJsonTextBox();
            };
            
            frequencyTextBox.TextChanged += (_, _) => {
                frequencyTextBox.Text = frequencyTextBox.Text.Replace(" ", "");
                if (double.TryParse(frequencyTextBox.Text, out double value) && value >= 0)
                {
                    frequencySlider.Value = value;
                    if (objectCombo.SelectedItem != null)
                    {
                        CustomPlacement.FrequencyAdjustments[
                            (string)(objectCombo.SelectedItem as ComboBoxItem).Content] = (float)value / 100;
                        UpdateJsonTextBox();
                    }
                }
            };
            
            objectCombo.SelectionChanged += (_, _) =>
            {
                CustomPlacement.FrequencyAdjustments.Clear();
                foreach (Control frequencyRow in FrequencyRowsContainer.Children)
                {
                    var comboBox = (frequencyRow as StackPanel).Children[1] as ComboBox;
                    var frequencyRowSlider = (frequencyRow as StackPanel).Children[2] as Slider;
                    if ((string)(comboBox.SelectedItem as ComboBoxItem).Content == "")
                    {
                        continue;
                    }
                    CustomPlacement.FrequencyAdjustments[(string)(comboBox.SelectedItem as ComboBoxItem).Content] = (float)frequencyRowSlider.Value / 100;
                }
                UpdateJsonTextBox();
            };

            row.Children.Add(removeButton);
            row.Children.Add(objectCombo);
            row.Children.Add(frequencySlider);
            row.Children.Add(frequencyTextBox);
            row.Children.Add(percentLabel);


            return row;
        }

        private void RemoveFrequencyRow(StackPanel row)
        {
            FrequencyRowsContainer.Children.Remove(row);
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

        private void RemoveCustomPlacementRow(string objectName, StackPanel row)
        {
            CustomPlacementRowsContainer.Children.Remove(row);
            if (objectName != null)
            {
                CustomPlacement.RemoveCustomPlacement(SelectedObjectForCustomPlacement, objectName);
                UpdateJsonTextBox();
            }
        }

        private void LoadCustomPlacementRows(string objectName)
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
}