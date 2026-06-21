using System.Windows;
using System.Windows.Controls;

namespace NebulaControls.Controls;

public class NebulaTextBox : TextBox
{
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(NebulaTextBox),
            new PropertyMetadata(string.Empty));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(NebulaTextBox),
            new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty LabelPlacementProperty =
        DependencyProperty.Register(
            nameof(LabelPlacement),
            typeof(Dock),
            typeof(NebulaTextBox),
            new PropertyMetadata(Dock.Top));

    public Dock LabelPlacement
    {
        get => (Dock)GetValue(LabelPlacementProperty);
        set => SetValue(LabelPlacementProperty, value);
    }

    public static readonly DependencyProperty HelperTextProperty =
        DependencyProperty.Register(
            nameof(HelperText),
            typeof(string),
            typeof(NebulaTextBox),
            new PropertyMetadata(string.Empty));

    public string HelperText
    {
        get => (string)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    public static readonly DependencyProperty ErrorTextProperty =
        DependencyProperty.Register(
            nameof(ErrorText),
            typeof(string),
            typeof(NebulaTextBox),
            new PropertyMetadata(string.Empty));

    public string ErrorText
    {
        get => (string)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    public static readonly DependencyProperty WarningTextProperty =
        DependencyProperty.Register(
            nameof(WarningText),
            typeof(string),
            typeof(NebulaTextBox),
            new PropertyMetadata(string.Empty));

    public string WarningText
    {
        get => (string)GetValue(WarningTextProperty);
        set => SetValue(WarningTextProperty, value);
    }

    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(
            nameof(HasError),
            typeof(bool),
            typeof(NebulaTextBox),
            new PropertyMetadata(false));

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public static readonly DependencyProperty HasWarningProperty =
        DependencyProperty.Register(
            nameof(HasWarning),
            typeof(bool),
            typeof(NebulaTextBox),
            new PropertyMetadata(false));

    public bool HasWarning
    {
        get => (bool)GetValue(HasWarningProperty);
        set => SetValue(HasWarningProperty, value);
    }
}
