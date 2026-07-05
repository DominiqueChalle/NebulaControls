# NebulaControls Usage

## Reference

Add a reference to the `NebulaControls` assembly.

During development inside this repository, the public demo uses a project reference so control changes can be tested immediately:

```xml
<ProjectReference Include="..\..\src\NebulaControls\NebulaControls.csproj" />
```

External applications should reference the built DLL or NuGet package instead:

```xml
<PackageReference Include="NebulaControls" Version="1.0.1-beta" />
```

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

## Buttons

Button V2 styles include action variants:

```xml
<Button Content="Primary" Style="{StaticResource NebulaPrimaryButton}" />
<Button Content="Secondary" Style="{StaticResource NebulaSecondaryButton}" />
<Button Content="Danger" Style="{StaticResource NebulaDangerButton}" />
<Button Content="Warning" Style="{StaticResource NebulaWarningButton}" />
<Button Content="Ghost" Style="{StaticResource NebulaGhostButton}" />
<Button Content="Subtle" Style="{StaticResource NebulaSubtleButton}" />
```

Each variant also exposes small and large styles, for example:

```xml
<Button Content="Small" Style="{StaticResource NebulaPrimarySmallButton}" />
<Button Content="Large" Style="{StaticResource NebulaPrimaryLargeButton}" />
```

## TextBox

Use `NebulaTextBox` for labeled fields with placeholder, helper text, and error text:

```xml
<nebula:NebulaTextBox
    Style="{StaticResource NebulaLabeledTextBox}"
    Label="Username"
    Placeholder="Enter username"
    HelperText="Displayed in your profile." />
```

Validation feedback can be shown with `HasError` and `ErrorText`:

```xml
<nebula:NebulaTextBox
    Style="{StaticResource NebulaLabeledTextBox}"
    Label="Email"
    Placeholder="name@example.com"
    HasError="True"
    ErrorText="Email is required." />
```

Native `TextBox` styles also expose small and large variants:

```xml
<TextBox Style="{StaticResource NebulaSmallTextBox}" />
<TextBox Style="{StaticResource NebulaMediumTextBox}" />
<TextBox Style="{StaticResource NebulaLargeTextBox}" />
```

## Theme Switching

`NebulaThemeManager` can replace the active Nebula theme dictionary at runtime:

```csharp
NebulaThemeManager.ApplyTheme(NebulaTheme.NebulaDarkPurple);
```

Available themes:

- `NebulaDarkPurple`
- `NebulaDarkBlue`
- `NebulaLightPurple`

Future themes should expose the same semantic resource keys before they are added to `NebulaTheme`.
