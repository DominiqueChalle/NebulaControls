// Nom: NebulaRating
// Version: V1.03
// Description: Rating control exposing value, maximum and change notification behavior.

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaRating : Control
{
    private readonly NebulaRelayCommand setRatingCommand;
    private readonly NebulaRelayCommand previewRatingCommand;
    private readonly NebulaRelayCommand clearPreviewCommand;

    public event EventHandler? ValueChanged;

    static NebulaRating()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaRating),
            new FrameworkPropertyMetadata(typeof(NebulaRating)));
    }

    public NebulaRating()
    {
        setRatingCommand = new NebulaRelayCommand(SetRating, _ => IsEnabled && !IsReadOnly);
        previewRatingCommand = new NebulaRelayCommand(PreviewRating, _ => IsEnabled && !IsReadOnly);
        clearPreviewCommand = new NebulaRelayCommand(_ => ClearPreview(), _ => IsEnabled && !IsReadOnly);
        Items = new ObservableCollection<NebulaRatingItem>();
        RefreshItems();
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(NebulaRating),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRatingStateChanged));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(int),
            typeof(NebulaRating),
            new PropertyMetadata(5, OnRatingStateChanged));

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(NebulaRating),
            new PropertyMetadata(false, OnInteractivityChanged));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty PreviewValueProperty =
        DependencyProperty.Register(
            nameof(PreviewValue),
            typeof(int),
            typeof(NebulaRating),
            new PropertyMetadata(0, OnPreviewValueChanged));

    public int PreviewValue
    {
        get => (int)GetValue(PreviewValueProperty);
        private set => SetValue(PreviewValueProperty, value);
    }

    public ObservableCollection<NebulaRatingItem> Items { get; }

    public ICommand SetRatingCommand => setRatingCommand;

    public ICommand PreviewRatingCommand => previewRatingCommand;

    public ICommand ClearPreviewCommand => clearPreviewCommand;

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (!IsEnabled || IsReadOnly)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Left:
            case Key.Down:
                Value = Math.Max(0, Value - 1);
                e.Handled = true;
                break;

            case Key.Right:
            case Key.Up:
                Value = Math.Min(Maximum, Value + 1);
                e.Handled = true;
                break;

            case Key.Home:
                Value = 0;
                e.Handled = true;
                break;

            case Key.End:
                Value = Maximum;
                e.Handled = true;
                break;
        }
    }

    private static void OnRatingStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaRating rating)
        {
            rating.CoerceState();
            rating.RefreshItems();
            rating.RefreshCommands();

            if (e.Property == ValueProperty)
            {
                rating.ValueChanged?.Invoke(rating, EventArgs.Empty);
            }
        }
    }

    private static void OnPreviewValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaRating rating)
        {
            rating.RefreshItems();
        }
    }

    private static void OnInteractivityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaRating rating)
        {
            rating.ClearPreview();
            rating.RefreshCommands();
        }
    }

    private void SetRating(object? parameter)
    {
        if (parameter is NebulaRatingItem item)
        {
            SetRatingItem(item);
        }
    }

    internal void SetRatingItem(NebulaRatingItem? item)
    {
        if (item is not null && IsEnabled && !IsReadOnly)
        {
            Value = item.Value;
        }
    }

    private void PreviewRating(object? parameter)
    {
        if (parameter is NebulaRatingItem item)
        {
            PreviewRatingItem(item);
        }
    }

    internal void PreviewRatingItem(NebulaRatingItem? item)
    {
        if (item is not null && IsEnabled && !IsReadOnly)
        {
            PreviewValue = item.Value;
        }
    }

    internal void ClearRatingPreview()
    {
        ClearPreview();
    }

    private void ClearPreview()
    {
        PreviewValue = 0;
    }

    private void CoerceState()
    {
        var maximum = Math.Clamp(Maximum, 1, 10);
        if (maximum != Maximum)
        {
            SetCurrentValue(MaximumProperty, maximum);
        }

        var value = Math.Clamp(Value, 0, maximum);
        if (value != Value)
        {
            SetCurrentValue(ValueProperty, value);
        }

        var previewValue = Math.Clamp(PreviewValue, 0, maximum);
        if (previewValue != PreviewValue)
        {
            PreviewValue = previewValue;
        }
    }

    private void RefreshItems()
    {
        Items.Clear();

        var activeValue = PreviewValue > 0 ? PreviewValue : Value;
        for (var value = 1; value <= Maximum; value++)
        {
            Items.Add(new NebulaRatingItem(value, value <= Value, value <= activeValue));
        }
    }

    private void RefreshCommands()
    {
        setRatingCommand.RaiseCanExecuteChanged();
        previewRatingCommand.RaiseCanExecuteChanged();
        clearPreviewCommand.RaiseCanExecuteChanged();
    }
}
