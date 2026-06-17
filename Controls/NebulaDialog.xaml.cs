using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NebulaUI.Controls;

public partial class NebulaDialog : Window
{
    public static readonly DependencyProperty DialogTitleProperty =
        DependencyProperty.Register(
            nameof(DialogTitle),
            typeof(string),
            typeof(NebulaDialog),
            new PropertyMetadata("Nebula dialog"));

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(NebulaDialog),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty PrimaryButtonTextProperty =
        DependencyProperty.Register(
            nameof(PrimaryButtonText),
            typeof(string),
            typeof(NebulaDialog),
            new PropertyMetadata("OK", OnButtonTextChanged));

    public static readonly DependencyProperty SecondaryButtonTextProperty =
        DependencyProperty.Register(
            nameof(SecondaryButtonText),
            typeof(string),
            typeof(NebulaDialog),
            new PropertyMetadata("Cancel", OnButtonTextChanged));

    public static readonly DependencyProperty TertiaryButtonTextProperty =
        DependencyProperty.Register(
            nameof(TertiaryButtonText),
            typeof(string),
            typeof(NebulaDialog),
            new PropertyMetadata(null, OnButtonTextChanged));

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(
            nameof(Variant),
            typeof(NebulaDialogVariant),
            typeof(NebulaDialog),
            new PropertyMetadata(NebulaDialogVariant.Info, OnVariantChanged));

    public NebulaDialog()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            UpdateButtonVisibility();
            UpdateVariant();
        };
    }

    public string DialogTitle
    {
        get => (string)GetValue(DialogTitleProperty);
        set => SetValue(DialogTitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string PrimaryButtonText
    {
        get => (string)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    public string SecondaryButtonText
    {
        get => (string)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    public string? TertiaryButtonText
    {
        get => (string?)GetValue(TertiaryButtonTextProperty);
        set => SetValue(TertiaryButtonTextProperty, value);
    }

    public NebulaDialogVariant Variant
    {
        get => (NebulaDialogVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public NebulaDialogResult Result { get; private set; } = NebulaDialogResult.None;

    private static void OnButtonTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaDialog dialog && dialog.IsLoaded)
        {
            dialog.UpdateButtonVisibility();
        }
    }

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaDialog dialog && dialog.IsLoaded)
        {
            dialog.UpdateVariant();
        }
    }

    private void UpdateButtonVisibility()
    {
        PrimaryButton.Visibility = string.IsNullOrWhiteSpace(PrimaryButtonText)
            ? Visibility.Collapsed
            : Visibility.Visible;
        SecondaryButton.Visibility = string.IsNullOrWhiteSpace(SecondaryButtonText)
            ? Visibility.Collapsed
            : Visibility.Visible;
        TertiaryButton.Visibility = string.IsNullOrWhiteSpace(TertiaryButtonText)
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void UpdateVariant()
    {
        var brushKey = Variant switch
        {
            NebulaDialogVariant.Success => "Brush.Success",
            NebulaDialogVariant.Warning => "Brush.Warning",
            NebulaDialogVariant.Danger => "Brush.Danger",
            _ => "Brush.Info"
        };

        var brush = (Brush)FindResource(brushKey);
        VariantRail.Background = brush;
        IconCircle.Fill = brush;
        IconPath.Stroke = brush;
        InfoIconText.Foreground = brush;

        InfoIconText.Visibility = Visibility.Collapsed;
        IconPath.Visibility = Visibility.Visible;

        IconPath.Data = Variant switch
        {
            NebulaDialogVariant.Success => Geometry.Parse("M 2 9 L 7 14 L 16 4"),
            NebulaDialogVariant.Warning => Geometry.Parse("M 9 2 L 16 15 H 2 Z M 9 6 V 10 M 9 13 H 9.1"),
            NebulaDialogVariant.Danger => Geometry.Parse("M 4 4 L 14 14 M 14 4 L 4 14"),
            _ => null
        };

        if (Variant == NebulaDialogVariant.Info)
        {
            IconPath.Visibility = Visibility.Collapsed;
            InfoIconText.Visibility = Visibility.Visible;
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void NebulaDialog_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CloseWithResult(NebulaDialogResult.Close, false);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        CloseWithResult(NebulaDialogResult.Close, false);
    }

    private void PrimaryButton_Click(object sender, RoutedEventArgs e)
    {
        CloseWithResult(NebulaDialogResult.Primary, true);
    }

    private void SecondaryButton_Click(object sender, RoutedEventArgs e)
    {
        CloseWithResult(NebulaDialogResult.Secondary, false);
    }

    private void TertiaryButton_Click(object sender, RoutedEventArgs e)
    {
        CloseWithResult(NebulaDialogResult.Tertiary, false);
    }

    private void CloseWithResult(NebulaDialogResult result, bool dialogResult)
    {
        Result = result;
        DialogResult = dialogResult;
    }
}
