using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace KappaCopy.App;

public sealed class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var request =
                StartupRequest.Parse(
                    Program.StartupArguments);

            switch (request.Action)
            {
                case StartupAction.Copy:
                    HandleCopyRequest(
                        desktop,
                        request.Path);
                    return;

                case StartupAction.Paste:
                    HandlePasteRequest(
                        desktop,
                        request.Path);
                    break;

                default:
                    desktop.MainWindow =
                        new MainWindow();
                    break;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void HandleCopyRequest(
        IClassicDesktopStyleApplicationLifetime desktop,
        string? path)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            ShellClipboardStore.AddFromShell(path);
        }

        /*
         * --copy deve essere invisibile.
         *
         * Explorer avvierà KappaCopy.App.exe,
         * noi registriamo il percorso e terminiamo.
         */

        desktop.Shutdown();
    }

    private static void HandlePasteRequest(
        IClassicDesktopStyleApplicationLifetime desktop,
        string? destination)
    {
        var window =
            new MainWindow();

        desktop.MainWindow = window;

        if (!string.IsNullOrWhiteSpace(destination))
        {
            window.LoadShellPaste(
                ShellClipboardStore.Load(),
                destination);
        }
    }
}