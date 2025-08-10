using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace E33Randomizer
{
    public partial class CustomPlacementWindow
    {
        public string SelectedObjectForCustomPlacement = null;
        private CustomPlacement CustomPlacement;

        public CustomPlacementWindow(CustomPlacement customPlacement)
        {
            CustomPlacement = customPlacement;
            InitializeComponent();
            PopulateEnemyDropdowns();
            Update();
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
            string json = JsonConvert.SerializeObject(presetData, Formatting.Indented);
            PresetJsonTextBox.Text = json;
        }
        
        private void UpdateExcludedListBox()
        {
            ExcludedEnemiesListBox.Items.Clear();
            foreach (var excludedOption in CustomPlacement.Excluded)
            {
                ExcludedEnemiesListBox.Items.Add(excludedOption);
            }
        }

        private void UpdateNotRandomizedListBox()
        {
            NotRandomizedEnemiesListBox.Items.Clear();
            foreach (var notRandomizedOption in CustomPlacement.NotRandomized)
            {
                NotRandomizedEnemiesListBox.Items.Add(notRandomizedOption);
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
        
        private void PopulateEnemyComboBox(ComboBox comboBox)
        {
            foreach (string enemy in CustomPlacement.PlainNamesList)
            {
                comboBox.Items.Add(new ComboBoxItem { Content = enemy });
            }
        }
        
        private void PopulateEnemyDropdowns()
        {
            PopulateEnemyComboBox(NotRandomizedEnemiesSelectionComboBox);
            PopulateEnemyComboBox(ExcludedEnemiesComboBox);
            PopulateEnemyComboBox(OopsAllEnemyComboBox);
            
            foreach (string enemy in CustomPlacement.PlainNamesList)
            {
                CustomPlacementEnemyListBox.Items.Add(new ComboBoxItem { Content = enemy });
            }
        }

        private void NotRandomizedEnemiesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotRandomizedEnemiesSelectionComboBox.SelectedItem != null)
            {
                AddNotRandomizedEnemy();
            }
        }

        private void AddNotRandomizedEnemy()
        {
            if (NotRandomizedEnemiesSelectionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string enemyName = selectedItem.Content.ToString();
                
                // Check if enemy is already in the list
                if (!CustomPlacement.NotRandomized.Contains(enemyName))
                {
                    CustomPlacement.AddNotRandomized(enemyName);
                    UpdateJsonTextBox();
                }
                
                // Reset selection
                NotRandomizedEnemiesSelectionComboBox.SelectedItem = null;
                UpdateNotRandomizedListBox();
            }
        }

        private void NotRandomizedEnemiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveNotRandomizedEnemyButton.IsEnabled = NotRandomizedEnemiesListBox.SelectedItem != null;
        }

        private void RemoveNotRandomizedEnemyButton_Click(object sender, RoutedEventArgs e)
        {
            if (NotRandomizedEnemiesListBox.SelectedItem != null)
            {
                string selectedEnemy = NotRandomizedEnemiesListBox.SelectedItem.ToString();

                CustomPlacement.RemoveNotRandomized(selectedEnemy);
                
                UpdateNotRandomizedListBox();
                UpdateJsonTextBox();
                RemoveNotRandomizedEnemyButton.IsEnabled = false;
            }
        }
        
        private void ExcludedEnemiesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExcludedEnemiesComboBox.SelectedItem != null)
            {
                AddExcludedEnemy();
            }
        }

        private void AddExcludedEnemy()
        {
            if (ExcludedEnemiesComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string enemyName = selectedItem.Content.ToString();
                
                // Check if enemy is already in the list
                if (!CustomPlacement.Excluded.Contains(enemyName))
                {
                    CustomPlacement.AddExcluded(enemyName);
                    UpdateExcludedListBox();
                    UpdateJsonTextBox();
                }
                else
                {
                    MessageBox.Show($"{enemyName} is already in the selected enemies list.", 
                                    "Duplicate Enemy", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // Reset selection
                ExcludedEnemiesComboBox.SelectedItem = null;
            }
        }

        private void ExcludedEnemiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveExcludedEnemyButton.IsEnabled = ExcludedEnemiesListBox.SelectedItem != null;
        }

        private void RemoveExcludedEnemyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExcludedEnemiesListBox.SelectedItem != null)
            {
                string selectedEnemy = ExcludedEnemiesListBox.SelectedItem.ToString();
                CustomPlacement.RemoveExcluded(selectedEnemy);
                UpdateExcludedListBox();
                UpdateJsonTextBox();
                RemoveExcludedEnemyButton.IsEnabled = false;
            }
        }

        private void OopsAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (OopsAllEnemyComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string enemyName = selectedItem.Content.ToString();
                CustomPlacement.CustomPlacementRules = new Dictionary<string, Dictionary<string, float>>()
                {
                    {"Anyone", new Dictionary<string, float>() {{enemyName, 1}}}
                };
                CustomPlacement.FrequencyAdjustments.Clear();
                CustomPlacement.Excluded.Clear();
                CustomPlacement.ExcludedCodeNames.Clear();
                
                CustomPlacement.NotRandomized.Clear();
                CustomPlacement.NotRandomizedCodeNames.Clear();
                
                LoadCustomPlacementRows(SelectedObjectForCustomPlacement);
                Update();
            }
            else
            {
                MessageBox.Show("Please select an enemy from the dropdown first.", 
                               "No Enemy Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: string presetFile })
            {
                try
                {
                    CustomPlacement.LoadFromJson(presetFile);
                    LoadCustomPlacementRows(SelectedObjectForCustomPlacement);
                    Update();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading preset: {ex.Message}; Reverting to Default.", 
                        "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CustomPlacement.LoadDefaultPreset();
                    UpdateJsonTextBox();
                }
            }
        }

        private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Load Custom Preset",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    CustomPlacement.LoadFromJson(openFileDialog.FileName);
                    LoadCustomPlacementRows(SelectedObjectForCustomPlacement);
                    Update();
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
                Title = "Save Custom Preset",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 1,
                DefaultExt = "json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    CustomPlacement.SaveToJson(saveFileDialog.FileName);
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

        private void AddFrequencyRowButton_Click(object sender, RoutedEventArgs e)
        {
            var newRow = CreateFrequencyRow("");
            FrequencyRowsContainer.Children.Add(newRow);
        }

        private StackPanel CreateFrequencyRow(string enemyName)
        {
            StackPanel row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 5)
            };


            // Enemy dropdown
            ComboBox enemyCombo = new ComboBox
            {
                Width = 150,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            PopulateEnemyComboBox(enemyCombo);
            
            // Remove button
            Button removeButton = new Button
            {
                Content = "-",
                Width = 30,
                Height = 30,
                Margin = new Thickness(0, 0, 5, 0),
                Background = System.Windows.Media.Brushes.Red,
                Foreground = System.Windows.Media.Brushes.White,
                FontWeight = FontWeights.Bold
            };
            removeButton.Click += (_, _) =>
            {
                var enemyName = (string)(enemyCombo.SelectedItem as ComboBoxItem)?.Content;
                if (enemyName != null)
                {
                    CustomPlacement.FrequencyAdjustments.Remove(enemyName);
                }
                RemoveFrequencyRow(row);
                UpdateJsonTextBox();
            };

            if (enemyName != "")
            {
                enemyCombo.SelectedIndex = CustomPlacement.PlainNamesList.IndexOf(enemyName);
            }

            Slider frequencySlider = new Slider
            {
                Width = 100,
                Height = 30,
                Minimum = 0,
                Maximum = 100,
                Value = enemyName == "" ? 100 : CustomPlacement.FrequencyAdjustments[enemyName] * 100,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Frequency text input
            TextBox frequencyTextBox = new TextBox
            {
                Width = 60,
                Height = 30,
                Text = enemyName == "" ? "100" : (CustomPlacement.FrequencyAdjustments[enemyName] * 100).ToString("F1"),
                VerticalContentAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Label percentLabel = new Label
            {
                Content = "%",
                VerticalAlignment = VerticalAlignment.Center
            };

            // Bind slider and textbox
            frequencySlider.ValueChanged += (_, e) => {
                if (!frequencyTextBox.IsFocused)
                {
                    frequencyTextBox.Text = e.NewValue.ToString("F1");
                    if (enemyCombo.SelectedItem != null)
                    {
                        CustomPlacement.FrequencyAdjustments[
                            (string)(enemyCombo.SelectedItem as ComboBoxItem).Content] = (float)e.NewValue / 100;
                        UpdateJsonTextBox();
                    }
                }
            };

            frequencyTextBox.PreviewTextInput += (_, e) =>
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
                UpdateJsonTextBox();
            };
            
            frequencyTextBox.TextChanged += (_, _) => {
                frequencyTextBox.Text = frequencyTextBox.Text.Replace(" ", "");
                if (double.TryParse(frequencyTextBox.Text, out double value) && value >= 0)
                {
                    frequencySlider.Value = value;
                    if (enemyCombo.SelectedItem != null)
                    {
                        CustomPlacement.FrequencyAdjustments[
                            (string)(enemyCombo.SelectedItem as ComboBoxItem).Content] = (float)value / 100;
                        UpdateJsonTextBox();
                    }
                }
            };
            
            enemyCombo.SelectionChanged += (_, _) =>
            {
                CustomPlacement.FrequencyAdjustments.Clear();
                foreach (UIElement frequencyRow in FrequencyRowsContainer.Children)
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
            row.Children.Add(enemyCombo);
            row.Children.Add(frequencySlider);
            row.Children.Add(frequencyTextBox);
            row.Children.Add(percentLabel);


            return row;
        }

        private void RemoveFrequencyRow(StackPanel row)
        {
            FrequencyRowsContainer.Children.Remove(row);
        }

        private void CustomPlacementEnemyListBox_SelectionChanged(object sender, MouseButtonEventArgs e)
        {
            var item = (ListBoxItem)sender;
            if (item.Content is string selectedEnemy)
            {
                SelectedObjectForCustomPlacement = selectedEnemy;
                SelectedEnemyDisplay.Text = $"Selected: {selectedEnemy}";
                AddCustomPlacementRowButton.IsEnabled = true;
                
                LoadCustomPlacementRows(selectedEnemy);
            }
            else
            {
                SelectedObjectForCustomPlacement = null;
                SelectedEnemyDisplay.Text = "No enemy selected";
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

        private StackPanel CreateCustomPlacementRow(string enemyName)
        {
            StackPanel row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 5)
            };


            ComboBox enemyNameCombo = new ComboBox
            {
                Width = 150,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0)
            };
            PopulateEnemyComboBox(enemyNameCombo);
            if (enemyName != "")
            {
                enemyNameCombo.SelectedIndex = CustomPlacement.PlainNamesList.IndexOf(enemyName);
            }
            
            // Remove button
            Button removeButton = new Button
            {
                Content = "-",
                Width = 30,
                Height = 30,
                Margin = new Thickness(0, 0, 5, 0),
                Background = System.Windows.Media.Brushes.Red,
                Foreground = System.Windows.Media.Brushes.White,
                FontWeight = FontWeights.Bold
            };
            removeButton.Click += (_, _) => RemoveCustomPlacementRow((string)(enemyNameCombo.SelectedItem as ComboBoxItem)?.Content, row);

            float frequency = 1;

            if (SelectedObjectForCustomPlacement != "" && CustomPlacement.CustomPlacementRules.ContainsKey(SelectedObjectForCustomPlacement) &&
                CustomPlacement.CustomPlacementRules[SelectedObjectForCustomPlacement].ContainsKey(enemyName))
            {
                frequency = CustomPlacement.CustomPlacementRules[SelectedObjectForCustomPlacement][enemyName];
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
    
            
            enemyNameCombo.SelectionChanged += (_, _) =>
            {
                CustomPlacement.SetCustomPlacement(SelectedObjectForCustomPlacement,
                    (string)(enemyNameCombo.SelectedItem as ComboBoxItem).Content, (float)frequencySlider.Value / 100); 
                UpdateJsonTextBox();
            };
            
            // Bind slider and textbox
            frequencySlider.ValueChanged += (_, e) => {
                if (!frequencyTextBox.IsFocused)
                {
                    frequencyTextBox.Text = e.NewValue.ToString("F1");
                }
                if (enemyNameCombo.SelectedItem != null)
                {
                    CustomPlacement.SetCustomPlacement(SelectedObjectForCustomPlacement, (string)(enemyNameCombo.SelectedItem as ComboBoxItem).Content, (float)e.NewValue / 100);
                    UpdateJsonTextBox();
                }
            };

            frequencyTextBox.PreviewTextInput += (_, e) =>
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            };

            frequencyTextBox.TextChanged += (_, _) => {
                frequencyTextBox.Text = frequencyTextBox.Text.Replace(" ", "");

                if (double.TryParse(frequencyTextBox.Text, out double value) && value >= 0)
                {
                    frequencySlider.Value = value;
                    if (enemyNameCombo.SelectedItem != null)
                    {
                        CustomPlacement.SetCustomPlacement(SelectedObjectForCustomPlacement,
                            (string)(enemyNameCombo.SelectedItem as ComboBoxItem).Content, (float)value / 100);
                        UpdateJsonTextBox();
                    }
                }
            };

            Label percentLabel = new Label
            {
                Content = "%"
            };

            row.Children.Add(removeButton);
            row.Children.Add(enemyNameCombo);
            row.Children.Add(frequencySlider);
            row.Children.Add(frequencyTextBox);
            row.Children.Add(percentLabel);

            return row;
        }

        private void RemoveCustomPlacementRow(string enemyName, StackPanel row)
        {
            CustomPlacementRowsContainer.Children.Remove(row);
            if (enemyName != null)
            {
                CustomPlacement.RemoveCustomPlacement(SelectedObjectForCustomPlacement, enemyName);
                UpdateJsonTextBox();
            }
        }

        private void LoadCustomPlacementRows(string enemyName)
        {
            if (enemyName == null)
            {
                return;
            }
            CustomPlacementRowsContainer.Children.Clear();
            if (!CustomPlacement.CustomPlacementRules.TryGetValue(enemyName, out var customPlacements))
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            
        }
    }
}