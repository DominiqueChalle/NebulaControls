using System;
using System.Linq;
using System.Windows;

namespace NebulaControls.Theming;

public static class NebulaThemeManager
{
    private const string AssemblyName = "NebulaControls";
    private const string ThemePathMarker = $"/{AssemblyName};component/Themes/";

    public static Uri GetThemeUri(NebulaTheme theme)
    {
        var themeName = theme switch
        {
            NebulaTheme.NebulaDarkPurple => "NebulaDarkPurple",
            NebulaTheme.NebulaDarkBlue => "NebulaDarkBlue",
            NebulaTheme.NebulaLightPurple => "NebulaLightPurple",
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
        };

        return new Uri(
            $"pack://application:,,,/{AssemblyName};component/Themes/{themeName}/Theme.xaml",
            UriKind.Absolute);
    }

    public static void ApplyTheme(NebulaTheme theme)
    {
        ApplyTheme(Application.Current, theme);
    }

    public static void ApplyTheme(Application application, NebulaTheme theme)
    {
        ArgumentNullException.ThrowIfNull(application);

        var themeDictionary = new ResourceDictionary
        {
            Source = GetThemeUri(theme)
        };

        var dictionaries = application.Resources.MergedDictionaries;
        var existingTheme = dictionaries.FirstOrDefault(IsNebulaThemeDictionary);

        if (existingTheme is not null)
        {
            var index = dictionaries.IndexOf(existingTheme);
            dictionaries[index] = themeDictionary;
            return;
        }

        dictionaries.Insert(0, themeDictionary);
    }

    private static bool IsNebulaThemeDictionary(ResourceDictionary dictionary)
    {
        return dictionary.Source?.OriginalString.Contains(ThemePathMarker, StringComparison.OrdinalIgnoreCase) == true;
    }
}
