using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using KappaCopy.Core;
using KappaCopy.Engine;

namespace KappaCopy.Engine.Windows;

public sealed partial class RobocopyEngine : ICopyEngine
{
    public bool IsSupported => OperatingSystem.IsWindows();

    public string DisplayName => "Robocopy";

    public async Task<CopyResult> CopyAsync(
        CopyJob job,
        IProgress<CopyProgress>? progress = null,
        Action<string>? log = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);

        if (!IsSupported)
        {
            return new CopyResult(
                false,
                false,
                -1,
                0,
                job.Items.Count,
                ["Robocopy è disponibile solo su Windows."]);
        }

        if (job.Items.Count == 0)
        {
            return new CopyResult(
                false,
                false,
                -1,
                0,
                0,
                ["Nessun file o cartella selezionato."]);
        }

        Directory.CreateDirectory(job.DestinationPath);

        var errors = new List<string>();
        var completed = 0;
        var finalExitCode = 0;

        try
        {
            for (var index = 0; index < job.Items.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = job.Items[index];

                if (!File.Exists(item.SourcePath) &&
                    !Directory.Exists(item.SourcePath))
                {
                    var error = $"Origine non trovata: {item.SourcePath}";

                    errors.Add(error);
                    log?.Invoke(error);

                    continue;
                }

                var startPercent =
                    (int)Math.Floor(index * 100d / job.Items.Count);

                progress?.Report(
                    new CopyProgress(
                        startPercent,
                        0,
                        completed,
                        job.Items.Count,
                        item.SourcePath,
                        "Preparazione..."));

                var invocation = BuildInvocation(
                    item,
                    job.DestinationPath,
                    job.Profile);

                log?.Invoke(
                    $"> robocopy {FormatForLog(invocation.Arguments)}");

                var exitCode = await RunRobocopyAsync(
                    invocation,
                    index,
                    job.Items.Count,
                    completed,
                    progress,
                    log,
                    cancellationToken);

                finalExitCode = Math.Max(
                    finalExitCode,
                    exitCode);

                // Robocopy:
                // 0..7 = nessun errore fatale
                // >= 8 = almeno un errore
                if (exitCode >= 8)
                {
                    var error =
                        $"Robocopy ha terminato '{item.Name}' " +
                        $"con codice {exitCode}.";

                    errors.Add(error);
                    log?.Invoke(error);
                }
                else
                {
                    completed++;
                }

                var overall =
                    (int)Math.Round(
                        (index + 1) * 100d /
                        job.Items.Count);

                progress?.Report(
                    new CopyProgress(
                        overall,
                        100,
                        completed,
                        job.Items.Count,
                        item.SourcePath,
                        "Elemento completato"));
            }
        }
        catch (OperationCanceledException)
        {
            progress?.Report(
                new CopyProgress(
                    0,
                    0,
                    completed,
                    job.Items.Count,
                    null,
                    "Copia annullata"));

            return new CopyResult(
                false,
                true,
                finalExitCode,
                completed,
                job.Items.Count,
                errors);
        }

        var success = errors.Count == 0;

        return new CopyResult(
            success,
            false,
            finalExitCode,
            completed,
            job.Items.Count,
            errors);
    }

    private static RobocopyInvocation BuildInvocation(
        CopyItem item,
        string destinationRoot,
        CopyProfile profile)
    {
        var arguments = new List<string>();

        if (item.IsDirectory)
        {
            var source = item.SourcePath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);

            var folderName = Path.GetFileName(source);

            if (string.IsNullOrWhiteSpace(folderName))
            {
                throw new InvalidOperationException(
                    $"Nome cartella sorgente non valido: {source}");
            }

            var destination =
                Path.Combine(
                    destinationRoot,
                    folderName);

            /*
             * IMPORTANTE:
             *
             * ArgumentList aggiunge ogni parametro separatamente.
             *
             * NON dobbiamo aggiungere manualmente:
             *
             * "C:\Percorso"
             *
             * ma semplicemente:
             *
             * C:\Percorso
             */

            arguments.Add(source);
            arguments.Add(destination);
            arguments.Add("/E");
        }
        else
        {
            var sourceDirectory =
                Path.GetDirectoryName(item.SourcePath);

            if (string.IsNullOrWhiteSpace(sourceDirectory))
            {
                throw new InvalidOperationException(
                    $"Directory sorgente non valida: {item.SourcePath}");
            }

            var fileName =
                Path.GetFileName(item.SourcePath);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException(
                    $"Nome file non valido: {item.SourcePath}");
            }

            arguments.Add(sourceDirectory);
            arguments.Add(destinationRoot);
            arguments.Add(fileName);
        }

        arguments.AddRange(
            GetProfileOptions(profile));

        return new RobocopyInvocation(arguments);
    }

    private static IReadOnlyList<string> GetProfileOptions(
        CopyProfile profile)
    {
        var options = new List<string>
        {
            "/COPY:DAT",
            "/DCOPY:DAT",
            "/R:2",
            "/W:1",
            "/TEE",
            "/BYTES",
            "/ETA"
        };

        switch (profile)
        {
            case CopyProfile.Fast:
                options.Add("/MT:16");
                options.Add("/J");
                break;

            case CopyProfile.Safe:
                options.Add("/MT:8");
                options.Add("/Z");
                break;

            default:
                options.Add("/MT:8");
                break;
        }

        return options;
    }

    private static async Task<int> RunRobocopyAsync(
        RobocopyInvocation invocation,
        int itemIndex,
        int totalItems,
        int completedItems,
        IProgress<CopyProgress>? progress,
        Action<string>? log,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "robocopy.exe",

            UseShellExecute = false,

            RedirectStandardOutput = true,
            RedirectStandardError = true,

            CreateNoWindow = true,

            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        /*
         * Questa è la correzione importante.
         *
         * Ogni parametro viene aggiunto separatamente.
         *
         * .NET si occupa automaticamente di:
         *
         * - spazi;
         * - virgolette;
         * - percorsi;
         * - caratteri speciali;
         * - escaping Windows.
         */

        foreach (var argument in invocation.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException(
                    "Impossibile avviare robocopy.exe.");
            }
        }
        catch (Exception ex)
        {
            log?.Invoke(
                $"Errore avvio Robocopy: {ex.Message}");

            return 16;
        }

        using var registration =
            cancellationToken.Register(
                () =>
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(
                                entireProcessTree: true);
                        }
                    }
                    catch
                    {
                        // Best effort durante annullamento.
                    }
                });

        var stdoutTask =
            PumpAsync(
                process.StandardOutput,
                false);

        var stderrTask =
            PumpAsync(
                process.StandardError,
                true);

        await process.WaitForExitAsync(
            cancellationToken);

        await Task.WhenAll(
            stdoutTask,
            stderrTask);

        return process.ExitCode;

        async Task PumpAsync(
            StreamReader reader,
            bool isError)
        {
            while (
                await reader.ReadLineAsync(
                    cancellationToken)
                is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                log?.Invoke(
                    isError
                        ? $"ERR: {line}"
                        : line);

                var currentPercent =
                    TryParsePercent(line);

                if (currentPercent is null)
                {
                    continue;
                }

                var overall =
                    (int)Math.Clamp(
                        Math.Round(
                            (
                                (
                                    itemIndex +
                                    currentPercent.Value /
                                    100d
                                )
                                /
                                totalItems
                            )
                            * 100d),
                        0,
                        100);

                progress?.Report(
                    new CopyProgress(
                        overall,
                        currentPercent.Value,
                        completedItems,
                        totalItems,
                        null,
                        "Copia in corso..."));
            }
        }
    }

    private static int? TryParsePercent(
        string line)
    {
        var match =
            PercentRegex().Match(line);

        if (!match.Success)
        {
            return null;
        }

        var rawValue =
            match.Groups[1]
                .Value
                .Replace(',', '.');

        if (!double.TryParse(
                rawValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var value))
        {
            return null;
        }

        return (int)Math.Clamp(
            Math.Round(value),
            0,
            100);
    }

    private static string FormatForLog(
        IReadOnlyList<string> arguments)
    {
        return string.Join(
            " ",
            arguments.Select(
                FormatArgumentForLog));
    }

    private static string FormatArgumentForLog(
        string argument)
    {
        /*
         * Queste virgolette servono SOLO
         * per mostrare un comando leggibile nel log.
         *
         * NON vengono passate a Robocopy.
         */

        if (argument.Contains(' ') ||
            argument.Contains('\t'))
        {
            return $"\"{argument}\"";
        }

        return argument;
    }

    [GeneratedRegex(
        @"(?<!\d)(\d{1,3}(?:[\.,]\d+)?)%")]
    private static partial Regex PercentRegex();

    private sealed record RobocopyInvocation(
        IReadOnlyList<string> Arguments);
}