using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace E33Randomizer;

public partial class MiscTab : UserControl
{
    private readonly MiscTabViewModel _viewModel;
    
    public MiscTab()
    {
        _viewModel = new MiscTabViewModel
        {
            DefaultSaveFilePath = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sandfall\\Saved\\SaveGames\\"),
                PlatformID.Unix => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Steam/steamapps/compatdata/1903340/pfx/drive_c/users/steamuser/AppData/Local/Sandfall/Saved/SaveGames/"),
                _ => throw new NotSupportedException()
            }
            
        };

        DataContext = _viewModel;
        
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
                string saveGamesBase = _viewModel.DefaultSaveFilePath;
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
                    new FilePickerFileType("SAV files (*.sav)") { Patterns = ["*.sav"] },
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
                        "Patched", MessageBoxButtons.Ok, MessageBoxIcons.Information);
                }
                catch (Exception ex)
                {
                    await MessageDialog.ShowAsync(topLevel, $"Error patching: {ex.Message}",
                        "Patching Error", MessageBoxButtons.Ok, MessageBoxIcons.Error);
                    await File.WriteAllTextAsync("crash_log.txt", ex.ToString(), Encoding.UTF8);
                }
            }
        }
        catch (Exception ex)
        {
            await MessageDialog.ShowAsync(topLevel, $"Error patching: {ex.Message}", "Error", MessageBoxButtons.Ok,  MessageBoxIcons.Error);
        }
    }
}


public class MiscTabViewModel : ObservableObject
{
    public string DefaultSaveFilePath { get; set; }
    public string SaveFileName => "EXPEDITION_[save slot id]";
}