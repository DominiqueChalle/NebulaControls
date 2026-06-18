using System.Windows;
using System.Windows.Controls;

namespace NebulaControls.Controls;

public class NebulaTextBox : TextBox
{
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
}
