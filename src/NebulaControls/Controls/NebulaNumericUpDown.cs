// Nom: NebulaNumericUpDown
// Version: V1.04
// Description: NumericUpDown control exposing value, minimum, maximum and increment properties.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaNumericUpDown : Control
{
    private readonly NebulaRelayCommand decreaseCommand;
    private readonly NebulaRelayCommand increaseCommand;
    private TextBox? textBox;

    public event EventHandler? ValueChanged;

    static NebulaNumericUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaNumericUpDown),
            new FrameworkPropertyMetadata(typeof(NebulaNumericUpDown)));
    }

    public NebulaNumericUpDown()
    {
        decreaseCommand = new NebulaRelayCommand(_ => ChangeValue(-Step), _ => CanDecrease());
        increaseCommand = new NebulaRelayCommand(_ => ChangeValue(Step), _ => CanIncrease());
        AddHandler(MouseWheelEvent, new MouseWheelEventHandler(HandleMouseWheel), true);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double?),
            typeof(NebulaNumericUpDown),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(NebulaNumericUpDown),
            new PropertyMetadata(double.MinValue, OnRangeChanged));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(NebulaNumericUpDown),
            new PropertyMetadata(double.MaxValue, OnRangeChanged));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(NebulaNumericUpDown),
            new PropertyMetadata(1d, OnStepChanged));

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(NebulaNumericUpDown),
            new PropertyMetadata(string.Empty));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(NebulaNumericUpDown),
            new PropertyMetadata(false, OnReadOnlyChanged));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public ICommand DecreaseCommand => decreaseCommand;

    public ICommand IncreaseCommand => increaseCommand;

    public override void OnApplyTemplate()
    {
        if (textBox is not null)
        {
            textBox.LostFocus -= TextBox_LostFocus;
            textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
        }

        base.OnApplyTemplate();

        textBox = GetTemplateChild("PART_TextBox") as TextBox;

        if (textBox is not null)
        {
            textBox.LostFocus += TextBox_LostFocus;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
        }

        UpdateText();
        RefreshCommands();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaNumericUpDown numericUpDown)
        {
            numericUpDown.CoerceValueInRange();
            numericUpDown.UpdateText();
            numericUpDown.RefreshCommands();
            numericUpDown.ValueChanged?.Invoke(numericUpDown, EventArgs.Empty);
        }
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaNumericUpDown numericUpDown)
        {
            numericUpDown.CoerceValueInRange();
            numericUpDown.RefreshCommands();
        }
    }

    private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaNumericUpDown numericUpDown)
        {
            numericUpDown.RefreshCommands();
        }
    }

    private static void OnReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaNumericUpDown numericUpDown)
        {
            numericUpDown.RefreshCommands();
        }
    }

    private void ChangeValue(double delta)
    {
        var currentValue = Value ?? 0d;
        Value = Clamp(currentValue + delta);
    }

    private bool CanDecrease()
    {
        return IsEnabled && !IsReadOnly && Step > 0 && (Value ?? 0d) > Minimum;
    }

    private bool CanIncrease()
    {
        return IsEnabled && !IsReadOnly && Step > 0 && (Value ?? 0d) < Maximum;
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        CommitText();
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            CommitText();
            return;
        }

        if (e.Key == Key.Enter)
        {
            CommitText();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            UpdateText();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Up)
        {
            CommitText();
            ChangeValue(Step);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Down)
        {
            CommitText();
            ChangeValue(-Step);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.PageUp)
        {
            CommitText();
            ChangeValue(GetLargeStep());
            e.Handled = true;
            return;
        }

        if (e.Key == Key.PageDown)
        {
            CommitText();
            ChangeValue(-GetLargeStep());
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Home)
        {
            Value = Minimum;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.End)
        {
            Value = Maximum;
            e.Handled = true;
        }
    }

    private void HandleMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled || !IsEnabled || IsReadOnly)
        {
            return;
        }

        CommitText();
        ChangeValue(e.Delta > 0 ? Step : -Step);
        e.Handled = true;
    }

    private void CommitText()
    {
        if (textBox is null || IsReadOnly)
        {
            UpdateText();
            return;
        }

        var text = textBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            Value = null;
            return;
        }

        if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var parsedValue)
            && !double.IsNaN(parsedValue)
            && !double.IsInfinity(parsedValue))
        {
            Value = Clamp(parsedValue);
            return;
        }

        UpdateText();
    }

    private void CoerceValueInRange()
    {
        if (Value is null)
        {
            return;
        }

        var coercedValue = Clamp(Value.Value);
        if (!coercedValue.Equals(Value.Value))
        {
            SetCurrentValue(ValueProperty, coercedValue);
        }
    }

    private double Clamp(double value)
    {
        var min = Math.Min(Minimum, Maximum);
        var max = Math.Max(Minimum, Maximum);
        return Math.Min(Math.Max(value, min), max);
    }

    private void UpdateText()
    {
        if (textBox is not null)
        {
            textBox.Text = Value?.ToString("G", CultureInfo.CurrentCulture) ?? string.Empty;
        }
    }

    private void RefreshCommands()
    {
        decreaseCommand.RaiseCanExecuteChanged();
        increaseCommand.RaiseCanExecuteChanged();
    }

    private double GetLargeStep()
    {
        return Step > 0
            ? Step * 10d
            : 10d;
    }
}
