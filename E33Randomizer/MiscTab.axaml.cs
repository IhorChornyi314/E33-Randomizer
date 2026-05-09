using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace E33Randomizer;

public partial class MiscTab : UserControl
{
    public MiscTab()
    {
        InitializeComponent();
    }
    
    
    private async void EnableCountersInSaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window topLevel) return;
        
        try
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

            var storage = topLevel.StorageProvider;

            var suggestedStartLocation = await storage.TryGetFolderFromPathAsync(targetFolder);
        
            var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a File",
                AllowMultiple = false,
                SuggestedStartLocation = suggestedStartLocation,
                FileTypeFilter =
                [
                    new FilePickerFileType("SAV files (*.sav)") { Patterns = ["*.save"] },
                    new FilePickerFileType("All Files") { Patterns = ["*"] }
                ]
            });

            if (files.Count == 1)
            {
                try
                {
                    switch ((sender as Button).Tag as string)
                    {
                        case "AddCounters":
                            SaveFilePatcher.AddCounters(files[0].Path.LocalPath);
                            break;
                        case "FixCurtain":
                            SaveFilePatcher.FixCurtain(files[0].Path.LocalPath);
                            break;
                    }
                
                    await MessageDialog.ShowAsync(topLevel, $"Save File Patched!",
                        "Patched", nameof(DialogBoxButton.OK), MessageBoxIcons.Information);
                }
                catch (Exception ex)
                {
                    await MessageDialog.ShowAsync(topLevel, $"Error patching: {ex.Message}",
                        "Patching Error", nameof(DialogBoxButton.OK), MessageBoxIcons.Error);
                    await File.WriteAllTextAsync("crash_log.txt", ex.ToString(), Encoding.UTF8);
                }
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(topLevel, $"Error patching: {ex.Message}", "Error", nameof(DialogBoxButton.OK),  MessageBoxIcons.Error);
        }
    }
}