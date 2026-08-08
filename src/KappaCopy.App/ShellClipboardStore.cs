using System.Text.Json;

namespace KappaCopy.App;

public static class ShellClipboardStore
{
    private static readonly string AppDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "KappaCopy");

    private static readonly string ClipboardPath =
        Path.Combine(
            AppDirectory,
            "clipboard.json");

    private const string MutexName =
        @"Local\KappaCopy_ShellClipboard_Mutex";

    public static void Replace(IEnumerable<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var normalized = NormalizePaths(paths);

        ExecuteLocked(() =>
        {
            SaveInternal(
                new ShellClipboardData
                {
                    Paths = normalized,
                    UpdatedUtc = DateTime.UtcNow
                });
        });
    }

    public static void AddFromShell(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        ExecuteLocked(() =>
        {
            var normalizedPath = NormalizePath(path);

            if (normalizedPath is null)
                return;

            var data = LoadInternal();

            /*
             * Explorer può avviare KappaCopy.App.exe una volta
             * per ogni elemento della stessa selezione.
             *
             * Le invocazioni arrivano quasi contemporaneamente.
             *
             * Se l'ultimo aggiornamento è vecchio, significa
             * che questa è una NUOVA operazione "Kappa Copy":
             * il vecchio clipboard va quindi cancellato.
             */
            var now = DateTime.UtcNow;

            var isNewCopyOperation =
                data.Paths.Count == 0 ||
                now - data.UpdatedUtc > TimeSpan.FromSeconds(3);

            if (isNewCopyOperation)
            {
                data.Paths.Clear();
            }

            if (!data.Paths.Any(
                    x => string.Equals(
                        x,
                        normalizedPath,
                        StringComparison.OrdinalIgnoreCase)))
            {
                data.Paths.Add(normalizedPath);
            }

            data.UpdatedUtc = now;

            SaveInternal(data);
        });
    }

    public static IReadOnlyList<string> Load()
    {
        IReadOnlyList<string> result = [];

        ExecuteLocked(() =>
        {
            result = LoadInternal()
                .Paths
                .Where(PathExists)
                .ToArray();
        });

        return result;
    }

    public static void Clear()
    {
        ExecuteLocked(() =>
        {
            try
            {
                if (File.Exists(ClipboardPath))
                    File.Delete(ClipboardPath);
            }
            catch
            {
                // Il clipboard non deve bloccare l'app.
            }
        });
    }

    public static bool HasItems()
    {
        return Load().Count > 0;
    }

    private static ShellClipboardData LoadInternal()
    {
        try
        {
            if (!File.Exists(ClipboardPath))
                return new ShellClipboardData();

            var json =
                File.ReadAllText(ClipboardPath);

            return JsonSerializer.Deserialize<ShellClipboardData>(json)
                   ?? new ShellClipboardData();
        }
        catch
        {
            return new ShellClipboardData();
        }
    }

    private static void SaveInternal(
        ShellClipboardData data)
    {
        try
        {
            Directory.CreateDirectory(AppDirectory);

            var json =
                JsonSerializer.Serialize(
                    data,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                ClipboardPath,
                json);
        }
        catch
        {
            // Best effort.
        }
    }

    private static List<string> NormalizePaths(
        IEnumerable<string> paths)
    {
        var result = new List<string>();

        foreach (var path in paths)
        {
            var normalized =
                NormalizePath(path);

            if (normalized is null)
                continue;

            if (result.Any(
                    x => string.Equals(
                        x,
                        normalized,
                        StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            result.Add(normalized);
        }

        return result;
    }

    private static string? NormalizePath(
        string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            var trimmed =
                path.Trim().Trim('"');

            if (!PathExists(trimmed))
                return null;

            return Path.GetFullPath(trimmed);
        }
        catch
        {
            return null;
        }
    }

    private static bool PathExists(
        string path)
    {
        return File.Exists(path)
               || Directory.Exists(path);
    }

    private static void ExecuteLocked(
        Action action)
    {
        using var mutex =
            new Mutex(
                false,
                MutexName);

        var lockTaken = false;

        try
        {
            try
            {
                lockTaken =
                    mutex.WaitOne(
                        TimeSpan.FromSeconds(5));
            }
            catch (AbandonedMutexException)
            {
                lockTaken = true;
            }

            if (!lockTaken)
                return;

            action();
        }
        finally
        {
            if (lockTaken)
            {
                try
                {
                    mutex.ReleaseMutex();
                }
                catch
                {
                    // Best effort.
                }
            }
        }
    }
}