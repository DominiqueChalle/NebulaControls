# NebulaControls Extraction Plan

## Goal

NebulaControls should become an independent WPF control library that can be consumed without the NebulaUI source code.

The target is a reusable library that can later be distributed as a DLL or NuGet package. NebulaUI should become only a gallery/demo application that consumes NebulaControls.

## Target Projects

```text
NebulaControls
  Independent WPF control library
  Contains controls, styles, templates, themes, and theme infrastructure

NebulaUI
  Demo/gallery application
  References NebulaControls
  Contains MainWindow, sample data, and gallery-only presentation
```

NebulaControls must not depend on NebulaUI.

## Theme Strategy

NebulaControls should be designed for multiple themes, even if only one theme exists at first.

Initial theme:

```text
NebulaDarkPurple
```

Future themes:

```text
NebulaLightPurple
NebulaDarkBlue
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
NebulaControls/
  NebulaControls.csproj
  Controls/
    NebulaButton.xaml
    NebulaTextBox.xaml
    NebulaDialog.xaml
    ...
  Themes/
    Common/
      Typography.xaml
      ControlResources.xaml
    NebulaDarkPurple/
      Colors.xaml
      Brushes.xaml
      Theme.xaml
    NebulaLightPurple/
      Colors.xaml
      Brushes.xaml
      Theme.xaml
    NebulaDarkBlue/
      Colors.xaml
      Brushes.xaml
      Theme.xaml
```

The first extraction can include only `NebulaDarkPurple`, while keeping the folder structure ready for future themes.

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

## What Stays In NebulaUI

- `MainWindow.xaml`
- gallery layout
- demo-only data
- demo-only event handlers
- `NebulaGalleryCard.xaml`

NebulaUI should become a consumer of NebulaControls, not the owner of the controls.

## Known Debts To Keep In Mind

- custom window corners remain a separate topic for a future `NebulaWindow`
- `NebulaTreeView` needs a full version later
- `NebulaDataGrid` needs editing and advanced scenarios later
- `NebulaSlider` disabled state is accepted but may need a better semantic disabled active brush
- `Accent*` resources still exist for validated controls and should not be removed blindly
- `NebulaPurpleBadge` and `NebulaPurpleAlert` may eventually be renamed to `NebulaBrandBadge` and `NebulaBrandAlert`

## Suggested Migration Steps

1. Create the independent `NebulaControls` WPF library project.
2. Add the target folder structure.
3. Move theme resources into `Themes/NebulaDarkPurple`.
4. Create `Themes/NebulaDarkPurple/Theme.xaml`.
5. Move reusable controls into the library.
6. Update namespaces from `NebulaUI.Controls` to the final library namespace.
7. Reference NebulaControls from NebulaUI.
8. Replace NebulaUI local resource dictionaries with pack URIs.
9. Keep `NebulaGalleryCard` and demo-only resources in NebulaUI.
10. Build and visually verify the gallery.

## Current Decisions

- Final namespace: `NebulaControls`.
- Whether controls should use explicit styles only, implicit styles, or both.
- `NebulaDialog` currently lives under `Controls/`; a future `Dialogs/` folder can be considered during packaging cleanup.
- Packaging strategy: DLL first, NuGet later.
