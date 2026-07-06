// Nom: NebulaChip
// Version: V1.03
// Description: Chip control exposing text, selection and remove command behavior.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaChip : ContentControl
{
    private readonly NebulaRelayCommand closeCommand;

    public event EventHandler? CloseClicked;

    static NebulaChip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaChip),
            new FrameworkPropertyMetadata(typeof(NebulaChip)));
    }

    public NebulaChip()
    {
        closeCommand = new NebulaRelayCommand(_ => CloseClicked?.Invoke(this, EventArgs.Empty), _ => IsEnabled && IsCloseVisible);
    }

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(NebulaChip),
            new PropertyMetadata(false));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly DependencyProperty IsCloseVisibleProperty =
        DependencyProperty.Register(
            nameof(IsCloseVisible),
            typeof(bool),
            typeof(NebulaChip),
            new PropertyMetadata(false, OnIsCloseVisibleChanged));

    public bool IsCloseVisible
    {
        get => (bool)GetValue(IsCloseVisibleProperty);
        set => SetValue(IsCloseVisibleProperty, value);
    }

    public ICommand CloseCommand => closeCommand;

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (IsEnabled && !IsCloseButtonSource(e.OriginalSource as DependencyObject))
        {
            IsSelected = !IsSelected;
        }
    }

    private static void OnIsCloseVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaChip chip)
        {
            chip.closeCommand.RaiseCanExecuteChanged();
        }
    }

    private static bool IsCloseButtonSource(DependencyObject? current)
    {
        while (current is not null)
        {
            if (current is Button)
            {
                return true;
            }

            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }

        return false;
    }
}
