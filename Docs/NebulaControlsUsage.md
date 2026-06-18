# NebulaControls Usage

## Reference

Add a reference to the `NebulaControls` assembly.

During development inside this solution, `NebulaUI` uses a project reference:

```xml
<ProjectReference Include="NebulaControls\NebulaControls.csproj" />
```

Later, an external application should reference the built DLL or NuGet package instead.

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

`Theme.xaml` contains theme resources such as colors, brushes, and typography.

`NebulaControls.xaml` contains reusable control styles and templates.

## XAML Namespace

Use the controls namespace where custom Nebula controls are needed:

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

`NebulaThemeManager` can replace the active Nebula theme dictionary at runtime:

```csharp
NebulaThemeManager.ApplyTheme(NebulaTheme.NebulaDarkPurple);
```

Only `NebulaDarkPurple` exists for now. Future themes should expose the same semantic resource keys before they are added to `NebulaTheme`.
