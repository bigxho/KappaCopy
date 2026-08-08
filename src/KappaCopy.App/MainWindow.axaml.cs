using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using KappaCopy.Core;
using KappaCopy.Engine;
using KappaCopy.Engine.Windows;
using KappaCopy.App.Localization;


namespace KappaCopy.App;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<SourceRow> _sources = [];
    private readonly ICopyEngine _copyEngine = new RobocopyEngine();
    private CancellationTokenSource? _copyCts;
    private Stopwatch? _stopwatch;
    private DispatcherTimer? _elapsedTimer;
    private long _totalBytes;
    private readonly AppSettings _settings;
    private bool _settingsLoaded;

    public MainWindow()
    {
        InitializeComponent();

        _settings = AppSettingsStore.Load();

        LanguageComboBox.SelectedIndex =
        _settings.Language == LocalizationManager.Italian ? 1 : 0;

        SourcesList.ItemsSource = _sources;

        EngineStatusText.Text =
            _copyEngine.IsSupported
                ? LocalizationManager.Get("EngineAvailable")
                : LocalizationManager.Get("EngineNotAvailable");

        CompletionSoundCheckBox.IsChecked =
            _settings.CompletionSoundEnabled;

        _settingsLoaded = true;

        Closing += MainWindow_Closing;

        UpdateSourceSummary();
        UpdateProfileDescription();

    }

    public async void LoadShellPaste(
        IReadOnlyList<string> paths,
        string destination)
    {
        _sources.Clear();

        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path))
                continue;

            if (_sources.Any(
                    x => string.Equals(
                        x.Path,
                        path,
                        StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (File.Exists(path))
            {
                long size = 0;

                try
                {
                    size =
                        new FileInfo(path).Length;
                }
                catch
                {
                    // Robocopy mostrerà eventuali problemi.
                }

                _sources.Add(
                    new SourceRow(
                        path,
                        false,
                        size));

                continue;
            }

            if (Directory.Exists(path))
            {
                _sources.Add(
                    new SourceRow(
                        path,
                        true,
                        null));
            }
        }

        DestinationTextBox.Text =
            destination;

        await RefreshSourceStatisticsAsync();

        if (_sources.Count == 0)
        {
            StatusText.Text =
                LocalizationManager.Get(
                "ClipboardEmpty");
        }
        else
        {
            StatusText.Text =
                LocalizationManager.Get("ClipboardNotEmpty").Replace("{0}", _sources.Count.ToString());
        }
    }

    private void MainWindow_Closing(
        object? sender,
        WindowClosingEventArgs e)
    {
        ShellClipboardStore.Clear();
    }

    private void LanguageComboBox_SelectionChanged(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (!_settingsLoaded)
            return;

        if (LanguageComboBox.SelectedItem
            is not ComboBoxItem item)
        {
            return;
        }

        var language =
            item.Tag?.ToString();

        if (string.IsNullOrWhiteSpace(language))
            return;

        LocalizationManager.SetLanguage(
            language);

        _settings.Language =
            language;

        AppSettingsStore.Save(
            _settings);

        RefreshLocalizedRuntimeText();
    }

    private void RefreshLocalizedRuntimeText()
    {
        EngineStatusText.Text =
            _copyEngine.IsSupported
                ? LocalizationManager.Get(
                    "EngineAvailable")
                : LocalizationManager.Get(
                    "EngineUnavailable");

        UpdateProfileDescription();
    }

    private void UpdateProfileDescription()
    {
        if (ProfileDescriptionText is null || ProfileComboBox is null)
        {
            return;
        }
        ProfileDescriptionText.Text =
            ProfileComboBox.SelectedIndex switch
            {
                1 => LocalizationManager.Get(
                    "ProfileFastDescription"),

                2 => LocalizationManager.Get(
                    "ProfileSafeDescription"),

                _ => LocalizationManager.Get(
                    "ProfileAutomaticDescription")
            };
    }

    private void CompletionSoundCheckBox_IsCheckedChanged(
        object? sender,
        RoutedEventArgs e)
        {
            if (!_settingsLoaded)
                return;

            _settings.CompletionSoundEnabled =
                CompletionSoundCheckBox.IsChecked == true;

            AppSettingsStore.Save(_settings);
        }

    private async void AddFiles_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Seleziona file da copiare",
            AllowMultiple = true
        });

        foreach (var file in files)
        {
            var path = GetLocalPath(file.Path);
            if (path is null || _sources.Any(x => string.Equals(x.Path, path, StringComparison.OrdinalIgnoreCase)))
                continue;

            long size = 0;
            try { size = new FileInfo(path).Length; } catch { }
            _sources.Add(new SourceRow(path, false, size));
        }

        await RefreshSourceStatisticsAsync();
    }

    private async void AddFolder_Click(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Seleziona cartella da copiare",
            AllowMultiple = true
        });

        foreach (var folder in folders)
        {
            var path = GetLocalPath(folder.Path);
            if (path is null || _sources.Any(x => string.Equals(x.Path, path, StringComparison.OrdinalIgnoreCase)))
                continue;

            _sources.Add(new SourceRow(path, true, null));
        }

        await RefreshSourceStatisticsAsync();
    }

    private async void PickDestination_Click(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Seleziona destinazione",
            AllowMultiple = false
        });

        if (folders.Count == 0)
            return;

        DestinationTextBox.Text = GetLocalPath(folders[0].Path) ?? folders[0].Path.ToString();
    }

    private async void RemoveSelected_Click(object? sender, RoutedEventArgs e)
    {
        if (SourcesList.SelectedItem is SourceRow row)
            _sources.Remove(row);

        await RefreshSourceStatisticsAsync();
    }

    private void ClearSources_Click(object? sender, RoutedEventArgs e)
    {
        _sources.Clear();
        _totalBytes = 0;
        UpdateSourceSummary();
    }

    private void ProfileComboBox_SelectionChanged(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (!_settingsLoaded)
            return;
        UpdateProfileDescription();
    }

    private async void StartCopy_Click(object? sender, RoutedEventArgs e)
    {
        if (_copyCts is not null)
            return;

        if (!_copyEngine.IsSupported)
        {
            StatusText.Text = 
            LocalizationManager.Get("RobocopyNotSupported");
            return;
        }

        if (_sources.Count == 0)
        {
            StatusText.Text = 
            LocalizationManager.Get("NoItemsSelected");
            return;
        }

        var destination = DestinationTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(destination))
        {
            StatusText.Text = 
            LocalizationManager.Get("DestinationNotSelected");
            return;
        }

        if (IsDestinationInsideSource(destination))
        {
            StatusText.Text = 
            LocalizationManager.Get("DestinationInsideSource");
            return;
        }

        var job = new CopyJob
        {
            Items = _sources.Select(x => new CopyItem(x.Path, x.IsDirectory)).ToArray(),
            DestinationPath = destination,
            Profile = GetSelectedProfile()
        };

        _copyCts = new CancellationTokenSource();
        SetCopyingState(true);
        LogTextBox.Text = string.Empty;
        OverallProgressBar.Value = 0;
        ProgressPercentText.Text = "0%";
        ItemsProgressText.Text = $"0 / {_sources.Count}";
        StatusText.Text = "Avvio Robocopy...";

        _stopwatch = Stopwatch.StartNew();
        _elapsedTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, (_, _) =>
        {
            ElapsedText.Text = FormatElapsed(_stopwatch?.Elapsed ?? TimeSpan.Zero);
        });
        _elapsedTimer.Start();

        var progress = new Progress<CopyProgress>(p =>
        {
            OverallProgressBar.Value = p.OverallPercent;
            ProgressPercentText.Text = $"{p.OverallPercent}%";
            ItemsProgressText.Text = $"{p.CompletedItems} / {p.TotalItems}";
            StatusText.Text = p.CurrentPath is null
                ? p.Message ?? LocalizationManager.Get("Copying")
                : $"{p.Message}  {Path.GetFileName(p.CurrentPath)}";
        });

        try
        {
            var result = await _copyEngine.CopyAsync(
                job,
                progress,
                AppendLog,
                _copyCts.Token);

            if (result.Cancelled)
            {
                StatusText.Text =
                    LocalizationManager.Get("CopyCancelled");
            }
            else if (result.Success)
            {
                OverallProgressBar.Value = 100;
                ProgressPercentText.Text = "100%";

                ItemsProgressText.Text =
                    $"{result.CompletedItems} / {result.TotalItems}";

                StatusText.Text =
                    LocalizationManager.Get("CopyCompleted").Replace("{0}", result.ExitCode.ToString());

                if (_settings.CompletionSoundEnabled)
                {
                    CompletionSoundService.Play();
                }
            }
            else
            {
                StatusText.Text = 
                LocalizationManager.Get("CopyFailed").Replace("{0}", result.ExitCode.ToString());
            }
        }
        catch (Exception ex)
        {
            AppendLog($"ERRORE: {ex}");
            StatusText.Text = 
            LocalizationManager.Get("CopyError").Replace("{0}", ex.Message);
        }
        finally
        {
            _stopwatch?.Stop();
            _elapsedTimer?.Stop();
            _elapsedTimer = null;
            _copyCts.Dispose();
            _copyCts = null;
            SetCopyingState(false);
            ElapsedText.Text = FormatElapsed(_stopwatch?.Elapsed ?? TimeSpan.Zero);
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        if (_copyCts is null)
            return;

        StatusText.Text = 
        LocalizationManager.Get("CopyCancelling");
        _copyCts.Cancel();
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void AppendLog(string line)
    {
        Dispatcher.UIThread.Post(() =>
        {
            LogTextBox.Text = string.IsNullOrEmpty(LogTextBox.Text)
                ? line
                : LogTextBox.Text + Environment.NewLine + line;
            LogTextBox.CaretIndex = LogTextBox.Text?.Length ?? 0;
        });
    }

    private async Task RefreshSourceStatisticsAsync()
    {
        UpdateSourceSummary(LocalizationManager.Get("CalculatingSizes"));

        var rows = _sources.ToArray();
        var sizes = await Task.Run(() => rows.Select(CalculateSizeSafely).ToArray());

        _totalBytes = 0;
        for (var i = 0; i < rows.Length; i++)
        {
            rows[i].SizeBytes = sizes[i];
            rows[i].SizeLabel = FormatBytes(sizes[i]);
            _totalBytes += sizes[i];
        }

        SourcesList.ItemsSource = null;
        SourcesList.ItemsSource = _sources;
        UpdateSourceSummary();
    }

    private static long CalculateSizeSafely(SourceRow row)
    {
        try
        {
            if (!row.IsDirectory)
                return new FileInfo(row.Path).Length;

            long total = 0;
            var pending = new Stack<string>();
            pending.Push(row.Path);

            while (pending.Count > 0)
            {
                var directory = pending.Pop();

                try
                {
                    foreach (var file in Directory.EnumerateFiles(directory))
                    {
                        try { total += new FileInfo(file).Length; } catch { }
                    }

                    foreach (var child in Directory.EnumerateDirectories(directory))
                        pending.Push(child);
                }
                catch
                {
                    // Directory non accessibile: Robocopy produrrà il dettaglio nel log.
                }
            }

            return total;
        }
        catch
        {
            return 0;
        }
    }

    private void UpdateSourceSummary(string? overrideText = null)
    {
        EmptySourcesText.IsVisible = _sources.Count == 0;
        SourceSummaryText.Text = overrideText ?? (_sources.Count == 0
            ? LocalizationManager.Get("NoItemsSelected")
            : $"{_sources.Count} {LocalizationManager.Get("Items") } • {FormatBytes(_totalBytes)}");
        TotalSizeText.Text = FormatBytes(_totalBytes);
        ItemsProgressText.Text = $"0 / {_sources.Count}";
    }

    private void SetCopyingState(bool copying)
    {
        CopyButton.IsEnabled = !copying;
        CloseButton.IsEnabled = !copying;
        CancelButton.IsEnabled = copying;
        SourcesList.IsEnabled = !copying;
        ProfileComboBox.IsEnabled = !copying;
    }

    private CopyProfile GetSelectedProfile() => ProfileComboBox.SelectedIndex switch
    {
        1 => CopyProfile.Fast,
        2 => CopyProfile.Safe,
        _ => CopyProfile.Automatic
    };

    private bool IsDestinationInsideSource(string destination)
    {
        string destinationFull;
        try { destinationFull = Path.GetFullPath(destination).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar; }
        catch { return false; }

        foreach (var source in _sources.Where(x => x.IsDirectory))
        {
            try
            {
                var sourceFull = Path.GetFullPath(source.Path).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (destinationFull.StartsWith(sourceFull, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            catch { }
        }

        return false;
    }

    private static string? GetLocalPath(Uri uri)
    {
        if (!uri.IsFile)
            return null;
        return uri.LocalPath;
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB", "PB"];
        double value = Math.Max(0, bytes);
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return unit == 0 ? $"{value:0} {units[unit]}" : $"{value:0.##} {units[unit]}";
    }

    private static string FormatElapsed(TimeSpan elapsed) =>
        elapsed.TotalHours >= 1
            ? $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}"
            : $"00:{elapsed.Minutes:00}:{elapsed.Seconds:00}";

    private sealed class SourceRow(string path, bool isDirectory, long? sizeBytes)
    {
        public string Path { get; } = path;
        public bool IsDirectory { get; } = isDirectory;
        public string TypeLabel => IsDirectory 
        ? LocalizationManager.Get("Directory") 
        : LocalizationManager.Get("File");
        public long SizeBytes { get; set; } = sizeBytes ?? 0;
        public string SizeLabel { get; set; } = sizeBytes.HasValue ? FormatBytes(sizeBytes.Value) : "...";
    }
}
