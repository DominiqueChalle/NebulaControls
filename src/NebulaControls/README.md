# NebulaControls

NebulaControls is a WPF control library containing reusable Nebula-themed controls, styles, templates, and theme resources.

The current default theme is `NebulaDarkPurple`. `NebulaDarkBlue` and `NebulaLightPurple` are also available.

## Local Package

Build a local NuGet package from the repository root:

```powershell
dotnet pack src/NebulaControls/NebulaControls.csproj
```

The package is written to `artifacts/packages`.

## App Resources

Load one theme dictionary and the global controls dictionary in `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/NebulaControls;component/Themes/NebulaDarkPurple/Theme.xaml" />
            <ResourceDictionary Source="pack://application:,,,/NebulaControls;component/Controls/NebulaControls.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## XAML Namespace

Use this namespace for custom Nebula controls:

```xml
xmlns:nebula="clr-namespace:NebulaControls.Controls;assembly=NebulaControls"
```

Example:

```xml
<nebula:NebulaListBox Style="{StaticResource NebulaListBox}" />
```

Native WPF controls use Nebula styles directly:

```xml
<Button Style="{StaticResource NebulaPrimaryButton}" />
<TextBox Style="{StaticResource NebulaTextBox}" />
```

## Theme Switching

The theme manager can replace the active Nebula theme dictionary at runtime:

```csharp
NebulaThemeManager.ApplyTheme(NebulaTheme.NebulaDarkPurple);
```

Available themes:

- `NebulaDarkPurple`
- `NebulaDarkBlue`
- `NebulaLightPurple`

Future themes should expose the same semantic resource keys before they are added to `NebulaTheme`.
