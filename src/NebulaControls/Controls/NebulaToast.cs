// Nom: NebulaToast
// Version: V1.03
// Description: Toast control exposing message, variant and close behavior.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaToast : ContentControl
{
    private readonly NebulaRelayCommand closeCommand;

    public event EventHandler? CloseClicked;

    static NebulaToast()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaToast),
            new FrameworkPropertyMetadata(typeof(NebulaToast)));
    }

    public NebulaToast()
    {
        closeCommand = new NebulaRelayCommand(_ => CloseClicked?.Invoke(this, EventArgs.Empty));
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(NebulaToast),
            new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(NebulaToast),
            new PropertyMetadata(string.Empty));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(string),
            typeof(NebulaToast),
            new PropertyMetadata("\uE946"));

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ICommand CloseCommand => closeCommand;
}
