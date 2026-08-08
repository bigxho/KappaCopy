using System.Runtime.InteropServices;

namespace KappaCopy.App;

public static class CompletionSoundService
{
    private const uint MB_OK = 0x00000000;

    public static void Play()
    {
        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            MessageBeep(MB_OK);
        }
        catch
        {
            // Il suono non deve mai bloccare l'app.
        }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MessageBeep(uint uType);
}