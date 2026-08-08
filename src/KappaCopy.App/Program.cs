using Avalonia;

namespace KappaCopy.App;

internal static class Program
{
    public static string[] StartupArguments { get; private set; } = [];

    [STAThread]
    public static void Main(string[] args)
    {
        StartupArguments = args;

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}