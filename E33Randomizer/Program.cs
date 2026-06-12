using Avalonia;

namespace E33Randomizer;

public sealed class Program
{
    public const string CrashLogFileName = "crash_log.txt";
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        FixExecutableBitOnLinux();
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static void FixExecutableBitOnLinux()
    {
        if (!OperatingSystem.IsLinux()) return;
        
        var mode = File.GetUnixFileMode($"{Directory.GetCurrentDirectory()}/retoc");
        if (!mode.HasFlag(UnixFileMode.UserExecute))
        {
            mode |= UnixFileMode.UserExecute;
            File.SetUnixFileMode($"{Directory.GetCurrentDirectory()}/retoc", mode);    
        }
            
            
        mode = File.GetUnixFileMode($"{Directory.GetCurrentDirectory()}/uesave");
        if (!mode.HasFlag(UnixFileMode.UserExecute))
        {
            mode |= UnixFileMode.UserExecute;
            File.SetUnixFileMode($"{Directory.GetCurrentDirectory()}/uesave", mode);    
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .ConfigureFonts(manager => { manager.AddFontCollection(new FontCollection()); })
            // .LogToTextWriter(File.CreateText("log.txt"), LogEventLevel.Verbose)
            .WithInterFont();
    }
}