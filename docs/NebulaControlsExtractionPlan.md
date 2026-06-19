# NebulaControls Extraction Plan

## Goal

NebulaControls should be an independent WPF control library that can be consumed without the demo source code.

The target is a reusable library that can be distributed as a DLL or NuGet package. The demo application should remain a sample consumer of NebulaControls.

## Target Projects

```text
src/NebulaControls
  Independent WPF control library.
  Contains controls, styles, templates, themes, and theme infrastructure.

samples/NebulaControls.Demo
  Public demo/gallery application.
  References src/NebulaControls during development.
  Contains MainWindow, sample data, and demo-only presentation.
```

NebulaControls must not depend on any demo application.

## Theme Strategy

NebulaControls should be designed for multiple themes, even if only one theme exists at first.

Initial theme:

```text
NebulaDarkPurple
```

Future themes:

```text
NebulaDarkBlue
NebulaLightPurple
```

Controls should use semantic resource keys, not theme-specific color names.

Examples:

```xml
Brush.Background
Brush.Surface
Brush.SurfaceHover
Brush.TextPrimary
Brush.Border
Brush.Brand
Brush.BrandHover
Brush.BrandPressed
Brush.Success
Brush.Warning
Brush.Danger
Brush.Info
```

Each theme provides the same keys with different values.

## Proposed Library Structure

```text
src/
  NebulaControls/
    NebulaControls.csproj
    Controls/
      NebulaButtons.xaml
      NebulaTextBox.xaml
      NebulaDialog.xaml
      ...
    Themes/
      Common/
        Typography.xaml
      NebulaDarkPurple/
        Colors.xaml
        Brushes.xaml
        Theme.xaml
      NebulaDarkBlue/
        Colors.xaml
        Brushes.xaml
        Theme.xaml
      NebulaLightPurple/
        .gitkeep
samples/
  NebulaControls.Demo/
    NebulaControls.Demo.csproj
    App.xaml
    MainWindow.xaml
docs/
artifacts/
  packages/
```

The current implementation includes `NebulaDarkPurple` and `NebulaDarkBlue`, while keeping the folder structure ready for future themes.

## Theme Import

An external WPF application should eventually be able to import a single theme dictionary:

```xml
<ResourceDictionary Source="pack://application:,,,/NebulaControls;component/Themes/NebulaDarkPurple/Theme.xaml"/>
```

`Theme.xaml` should merge:

- colors
- brushes
- typography

Reusable control styles are exposed through a single controls dictionary:

```xml
<ResourceDictionary Source="pack://application:,,,/NebulaControls;component/Controls/NebulaControls.xaml"/>
```

This keeps consumer setup simple while keeping theme resources separate from control templates. Dynamic theme switching can then replace the theme dictionary without reloading the controls dictionary.

## Dynamic Theme Switching

Dynamic theme switching should be possible later by replacing the loaded theme dictionary at runtime.

Requirements:

- controls use `DynamicResource` for theme-dependent resources
- all themes expose the same resource keys
- theme dictionaries are grouped in predictable locations
- avoid hard-coded colors in control templates

Initial API:

```csharp
NebulaThemeManager.ApplyTheme(NebulaTheme.NebulaDarkPurple);
```

Future themes should be added to `NebulaTheme` only when their matching `Theme.xaml` exists.

## What Moves To NebulaControls

- reusable control classes
- reusable control styles/templates
- theme resources
- `NebulaDialog`
- shared visual resources

Examples:

```text
Controls/NebulaButtons.xaml
Controls/NebulaTextBox.xaml
Controls/NebulaListBox.xaml
Controls/NebulaDialog.xaml
Themes/NebulaDarkPurple/*
```

## What Stays In The Demo

- `MainWindow.xaml`
- gallery layout
- demo-only data
- demo-only event handlers

The demo should remain a consumer of NebulaControls, not the owner of the controls.

## Known Debts To Keep In Mind

- custom window corners remain a separate topic for a future `NebulaWindow`
- `NebulaTreeView` needs a full version later
- `NebulaDataGrid` needs editing and advanced scenarios later
- `NebulaSlider` disabled state is accepted but may need a better semantic disabled active brush
- `Accent*` resources still exist for validated controls and should not be removed blindly
- `NebulaPurpleBadge` and `NebulaPurpleAlert` may eventually be renamed to `NebulaBrandBadge` and `NebulaBrandAlert`

## Suggested Migration Steps

1. Keep the independent `NebulaControls` WPF library under `src/NebulaControls`.
2. Keep the public demo under `samples/NebulaControls.Demo`.
3. Use a project reference from the demo during active development.
4. Use package references in external applications.
5. Build and visually verify the demo after control or theme changes.

## Current Decisions

- Final namespace: `NebulaControls`.
- The development demo uses a project reference.
- External applications should use a DLL or NuGet package.
- `NebulaDialog` currently lives under `Controls/`; a future `Dialogs/` folder can be considered during packaging cleanup.
- Packaging strategy: NuGet package from `src/NebulaControls`.
