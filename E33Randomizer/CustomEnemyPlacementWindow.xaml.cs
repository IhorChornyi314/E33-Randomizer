using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace E33Randomizer
{
    public partial class CustomEnemyPlacementWindow : Window
    {
        private bool hasUnsavedChanges;
        private string selectedEnemyForCustomPlacement = null;

        public CustomEnemyPlacementWindow()
        {
            InitializeComponent();
            PopulateEnemyDropdowns();
            SubscribeToChangeEvents();
            Update();
        }

        private void Update()
        {
            UpdateExcludedListBox();
            UpdateNotRandomizedListBox();
            UpdateFrequencies();
        }

        private void UpdateExcludedListBox()
        {
            ExcludedEnemiesListBox.Items.Clear();
            foreach (var excludedOption in CustomEnemyPlacement.Excluded)
            {
                ExcludedEnemiesListBox.Items.Add(excludedOption);
            }
        }

        private void UpdateNotRandomizedListBox()
        {
            NotRandomizedEnemiesListBox.Items.Clear();
            foreach (var notRandomizedOption in CustomEnemyPlacement.NotRandomized)
            {
                NotRandomizedEnemiesListBox.Items.Add(notRandomizedOption);
            }
        }

        private void UpdateFrequencies()
        {
            FrequencyRowsContainer.Children.Clear();
            foreach (var frequencyAdjustment in CustomEnemyPlacement.FrequencyAdjustments)
            {
                var newRow = CreateFrequencyRow(frequencyAdjustment.Key);
                FrequencyRowsContainer.Children.Add(newRow);
            }
        }
        
        private void SubscribeToChangeEvents()
        {
            OopsAllEnemyComboBox.SelectionChanged += (s, e) => hasUnsavedChanges = true;

            BossNumberCapCheckBox.Checked += (sender, args) => Settings.BossNumberCapped = true;
            BossNumberCapCheckBox.Unchecked += (sender, args) => Settings.BossNumberCapped = false;
            
            EnsureBossesInBossEncountersCheckBox.Checked += (sender, args) => Settings.EnsureBossesInBossEncounters = true;
            EnsureBossesInBossEncountersCheckBox.Unchecked += (sender, args) => Settings.EnsureBossesInBossEncounters = false;
            
            ReduceBossRepetitionCheckBox.Checked += (sender, args) => Settings.ReduceBossRepetition = true;
            ReduceBossRepetitionCheckBox.Unchecked += (sender, args) => Settings.ReduceBossRepetition = false;
        }
        
        private void PopulateEnemyComboBox(ComboBox comboBox)
        {
            string[] enemies = CustomEnemyPlacement.PlacementOptionsList.ToArray();
            
            foreach (string enemy in enemies)
            {
                comboBox.Items.Add(new ComboBoxItem { Content = enemy });
            }
        }
        
        private void PopulateEnemyDropdowns()
        {
            PopulateEnemyComboBox(NotRandomizedEnemiesSelectionComboBox);
            PopulateEnemyComboBox(ExcludedEnemiesComboBox);
            PopulateEnemyComboBox(OopsAllEnemyComboBox);
            
            string[] enemies = CustomEnemyPlacement.PlacementOptionsList.ToArray();
            foreach (string enemy in enemies)
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
                if (!CustomEnemyPlacement.NotRandomized.Contains(enemyName))
                {
                    CustomEnemyPlacement.NotRandomized.Add(enemyName);
                    hasUnsavedChanges = true;
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

                CustomEnemyPlacement.NotRandomized.Remove(selectedEnemy);
                hasUnsavedChanges = true;
                
                UpdateNotRandomizedListBox();
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
                if (!CustomEnemyPlacement.Excluded.Contains(enemyName))
                {
                    CustomEnemyPlacement.Excluded.Add(enemyName);
                    hasUnsavedChanges = true;
                    UpdateExcludedListBox();
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
                CustomEnemyPlacement.NotRandomized.Remove(selectedEnemy);
                hasUnsavedChanges = true;
                UpdateExcludedListBox();
                RemoveExcludedEnemyButton.IsEnabled = false;
            }
        }

        private void OopsAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (OopsAllEnemyComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string enemyName = selectedItem.Content.ToString();
                CustomEnemyPlacement.CustomPlacement = new Dictionary<string, Dictionary<string, float>>()
                {
                    {"Anyone", new Dictionary<string, float>() {{enemyName, 1}}}
                };
                hasUnsavedChanges = true;
                LoadCustomPlacementRows(selectedEnemyForCustomPlacement);
                CustomEnemyPlacement.UpdateFinalEnemyReplacementFrequencies();
            }
            else
            {
                MessageBox.Show("Please select an enemy from the dropdown first.", 
                               "No Enemy Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string presetFile)
            {
                
                try
                {
                    CustomEnemyPlacement.LoadFromJson(presetFile);
                    LoadCustomPlacementRows(selectedEnemyForCustomPlacement);
                    hasUnsavedChanges = true;
                    Update();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading preset: {ex.Message}; Reverting to Default.", 
                        "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CustomEnemyPlacement.LoadDefaultPreset();
                    hasUnsavedChanges = true;
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
                    CustomEnemyPlacement.LoadFromJson(openFileDialog.FileName);
                    LoadCustomPlacementRows(selectedEnemyForCustomPlacement);
                    hasUnsavedChanges = true;
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
                    CustomEnemyPlacement.SaveToJson(saveFileDialog.FileName);
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
            hasUnsavedChanges = true;
        }

        private StackPanel CreateFrequencyRow(String enemyName)
        {
            StackPanel row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 5)
            };

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
            removeButton.Click += (s, e) => RemoveFrequencyRow(row);

            // Enemy dropdown
            ComboBox enemyCombo = new ComboBox
            {
                Width = 150,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            PopulateEnemyComboBox(enemyCombo);

            if (enemyName != "")
            {
                enemyCombo.SelectedIndex = CustomEnemyPlacement.PlacementOptionsList.IndexOf(enemyName);
            }

            Slider frequencySlider = new Slider
            {
                Width = 100,
                Height = 30,
                Minimum = 0,
                Maximum = 100,
                Value = enemyName == "" ? 100 : CustomEnemyPlacement.FrequencyAdjustments[enemyName] * 100,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Frequency text input
            TextBox frequencyTextBox = new TextBox
            {
                Width = 60,
                Height = 30,
                Text = enemyName == "" ? "100" : (CustomEnemyPlacement.FrequencyAdjustments[enemyName] * 100).ToString("F1"),
                VerticalContentAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Label percentLabel = new Label
            {
                Content = "%",
                VerticalAlignment = VerticalAlignment.Center
            };

            // Bind slider and textbox
            frequencySlider.ValueChanged += (s, e) => {
                if (!frequencyTextBox.IsFocused)
                {
                    frequencyTextBox.Text = e.NewValue.ToString("F1");
                    hasUnsavedChanges = true;
                    if (enemyCombo.SelectedItem != null)
                    {
                        CustomEnemyPlacement.FrequencyAdjustments[
                            (String)(enemyCombo.SelectedItem as ComboBoxItem).Content] = (float)e.NewValue / 100;
                    }
                }
            };

            frequencyTextBox.PreviewTextInput += (s, e) =>
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            };
            
            frequencyTextBox.TextChanged += (s, e) => {
                if (double.TryParse(frequencyTextBox.Text, out double value) && value >= 0)
                {
                    frequencySlider.Value = value;
                    if (enemyCombo.SelectedItem != null)
                    {
                        CustomEnemyPlacement.FrequencyAdjustments[
                            (String)(enemyCombo.SelectedItem as ComboBoxItem).Content] = (float)value / 100;
                    }
                }
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
            hasUnsavedChanges = true;
        }

        private void CustomPlacementEnemyListBox_SelectionChanged(object sender, MouseButtonEventArgs e)
        {
            var item = (ListBoxItem)sender;
            if (item.Content is string selectedEnemy)
            {
                selectedEnemyForCustomPlacement = selectedEnemy;
                SelectedEnemyDisplay.Text = $"Selected: {selectedEnemy}";
                AddCustomPlacementRowButton.IsEnabled = true;
                
                LoadCustomPlacementRows(selectedEnemy);
            }
            else
            {
                selectedEnemyForCustomPlacement = null;
                SelectedEnemyDisplay.Text = "No enemy selected";
                AddCustomPlacementRowButton.IsEnabled = false;
                CustomPlacementRowsContainer.Children.Clear();
            }
        }

        private void AddCustomPlacementRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEnemyForCustomPlacement != null)
            {
                var row = CreateCustomPlacementRow("");
                CustomPlacementRowsContainer.Children.Add(row);
                hasUnsavedChanges = true;
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
                enemyNameCombo.SelectedIndex = CustomEnemyPlacement.PlacementOptionsList.IndexOf(enemyName);
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
            removeButton.Click += (s, e) => RemoveCustomPlacementRow((String)(enemyNameCombo.SelectedItem as ComboBoxItem)?.Content, row);

            float frequency = 1;

            if (selectedEnemyForCustomPlacement != "" && CustomEnemyPlacement.CustomPlacement.ContainsKey(selectedEnemyForCustomPlacement) &&
                CustomEnemyPlacement.CustomPlacement[selectedEnemyForCustomPlacement].ContainsKey(enemyName))
            {
                frequency = CustomEnemyPlacement.CustomPlacement[selectedEnemyForCustomPlacement][enemyName];
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
    
            
            enemyNameCombo.SelectionChanged += (sender, args) =>
            {
                CustomEnemyPlacement.SetCustomPlacement(selectedEnemyForCustomPlacement,
                    (String)(enemyNameCombo.SelectedItem as ComboBoxItem).Content, (float)frequencySlider.Value); 
            };
            
            // Bind slider and textbox
            frequencySlider.ValueChanged += (s, e) => {
                if (!frequencyTextBox.IsFocused)
                {
                    frequencyTextBox.Text = e.NewValue.ToString("F1");
                }
                hasUnsavedChanges = true;
                if (enemyNameCombo.SelectedItem != null)
                {
                    CustomEnemyPlacement.SetCustomPlacement(selectedEnemyForCustomPlacement, (String)(enemyNameCombo.SelectedItem as ComboBoxItem).Content, (float)e.NewValue / 100);
                }
            };

            frequencyTextBox.PreviewTextInput += (s, e) =>
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            };

            frequencyTextBox.TextChanged += (s, e) => {
                if (double.TryParse(frequencyTextBox.Text, out double value) && value >= 0)
                {
                    frequencySlider.Value = value;
                    if (enemyNameCombo.SelectedItem != null)
                    {
                        CustomEnemyPlacement.SetCustomPlacement(selectedEnemyForCustomPlacement,
                            (String)(enemyNameCombo.SelectedItem as ComboBoxItem).Content, (float)value / 100);
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
                CustomEnemyPlacement.RemoveCustomEnemyPlacement(selectedEnemyForCustomPlacement, enemyName);
            }
            hasUnsavedChanges = true;
        }

        private void LoadCustomPlacementRows(string enemyName)
        {
            if (enemyName == null)
            {
                return;
            }
            CustomPlacementRowsContainer.Children.Clear();
            if (!CustomEnemyPlacement.CustomPlacement.ContainsKey(enemyName))
            {
                return;
            }
            foreach (var pair in CustomEnemyPlacement.CustomPlacement[enemyName])
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