using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace E33Randomizer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static IApplicationLifetime? _applicationLifetime;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Assets.Resources.Culture = new CultureInfo("en-US");
        _applicationLifetime = ApplicationLifetime;
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static MainWindow GetMainWindow()
    {
        if (_applicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow as MainWindow ?? throw new Exception("Only desktop mode is supported");
        
        throw new  Exception("Only desktop mode is supported");
    }
}