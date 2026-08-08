using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KappaCopy.App.Localization;

public static class LocalizationManager
{
    public const string Italian = "it";
    public const string English = "en";

    private static ResourceDictionary? _currentDictionary;

    public static string CurrentLanguage { get; private set; } = English;

    public static void Initialize(string? language)
    {
        SetLanguage(NormalizeLanguage(language));
    }

    public static void SetLanguage(string language)
    {
        var normalized = NormalizeLanguage(language);

        if (Application.Current is null)
            return;

        var resources = Application.Current.Resources;

        if (_currentDictionary is not null)
        {
            resources.MergedDictionaries.Remove(_currentDictionary);
        }

        var uri = new Uri(
            $"avares://KappaCopy.App/Localization/Strings.{normalized}.axaml");

        _currentDictionary =
            AvaloniaXamlLoader.Load(uri) as ResourceDictionary
            ?? throw new InvalidOperationException(
                $"Unable to load localization resources: {uri}");

        resources.MergedDictionaries.Add(_currentDictionary);

        CurrentLanguage = normalized;

        var cultureName =
            normalized == Italian
                ? "it-IT"
                : "en-US";

        var culture = CultureInfo.GetCultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    public static string Get(string key)
    {
        if (_currentDictionary is not null &&
            _currentDictionary.TryGetValue(key, out var value) &&
            value is string text)
        {
            return text;
        }

        return key;
    }

    private static string NormalizeLanguage(string? language)
    {
        if (string.Equals(
                language,
                Italian,
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                language,
                "it-IT",
                StringComparison.OrdinalIgnoreCase))
        {
            return Italian;
        }

        return English;
    }
}
