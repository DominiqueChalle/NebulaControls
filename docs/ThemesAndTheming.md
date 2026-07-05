# Themes and Theming

NebulaControls uses two related folders for theme support:

- `src/NebulaControls/Themes`
- `src/NebulaControls/Theming`

They look similar by name, but they do not have the same role.

## `Themes`

`Themes` contains the visual resources used by the controls.

This folder is mostly XAML resource dictionaries:

- colors
- brushes
- typography
- theme dictionaries

Example:

```text
src/NebulaControls/Themes/NebulaDarkPurple/Theme.xaml
src/NebulaControls/Themes/NebulaDarkPurple/Colors.xaml
src/NebulaControls/Themes/NebulaDarkPurple/Brushes.xaml
src/NebulaControls/Themes/Common/Typography.xaml
```

Each theme exposes a `Theme.xaml` file that merges the resources needed by that visual theme.

## `Theming`

`Theming` contains the C# logic used to select and apply a theme at runtime.

This folder currently contains:

```text
src/NebulaControls/Theming/NebulaTheme.cs
src/NebulaControls/Theming/NebulaThemeManager.cs
```

`NebulaTheme` defines the available theme names.

`NebulaThemeManager` loads the matching XAML resource dictionary and replaces the current Nebula theme dictionary in the application resources.

Example:

```csharp
NebulaThemeManager.ApplyTheme(NebulaTheme.NebulaDarkPurple);
NebulaThemeManager.ApplyTheme(NebulaTheme.NebulaDarkBlue);
NebulaThemeManager.ApplyTheme(NebulaTheme.NebulaLightPurple);
```

## Short Version

- `Themes` = what the themes look like.
- `Theming` = how the application switches themes.

This separation keeps visual resources and runtime logic independent.
