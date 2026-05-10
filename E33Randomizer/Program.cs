using Avalonia;
using Avalonia.Logging;

namespace E33Randomizer;

public sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .ConfigureFonts(manager =>
            {
                manager.AddFontCollection(new  FontCollection());
            })
            .WithInterFont()
            .LogToTrace(LogEventLevel.Warning)
            // Add binding-specific verbose logging:
            .LogToTrace(LogEventLevel.Verbose, LogArea.Binding);
}