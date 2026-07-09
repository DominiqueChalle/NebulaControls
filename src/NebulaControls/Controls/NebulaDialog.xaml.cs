// Nom: NebulaDialog
// Version: V1.05
// Description: Dialog window logic exposing variants, content, convenience APIs, buttons and modal result behavior.

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NebulaControls.Controls;

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

    public static readonly DependencyProperty DialogContentProperty =
        DependencyProperty.Register(
            nameof(DialogContent),
            typeof(object),
            typeof(NebulaDialog),
            new PropertyMetadata(null, OnDialogContentChanged));

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
            UpdateDialogContentVisibility();
            UpdateVariant();
            PrimaryButton.Focus();
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

    public object? DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value);
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

    public static NebulaDialogResult ShowModal(
        Window? owner,
        string title,
        string message,
        NebulaDialogVariant variant = NebulaDialogVariant.Info,
        string primaryButtonText = "OK",
        string? secondaryButtonText = "Cancel",
        string? tertiaryButtonText = null,
        string windowTitle = "Nebula Dialog",
        double width = 460,
        object? content = null)
    {
        var dialog = new NebulaDialog
        {
            Owner = owner,
            Title = windowTitle,
            Width = width,
            DialogTitle = title,
            Message = message,
            DialogContent = content,
            Variant = variant,
            PrimaryButtonText = primaryButtonText,
            SecondaryButtonText = secondaryButtonText ?? string.Empty,
            TertiaryButtonText = tertiaryButtonText
        };

        dialog.ShowDialog();
        return dialog.Result;
    }

    public static NebulaDialogResult ShowInfo(
        Window? owner,
        string title,
        string message,
        string primaryButtonText = "OK",
        string windowTitle = "NebulaDialog Info")
    {
        return ShowModal(
            owner,
            title,
            message,
            NebulaDialogVariant.Info,
            primaryButtonText,
            null,
            null,
            windowTitle);
    }

    public static NebulaDialogResult ShowSuccess(
        Window? owner,
        string title,
        string message,
        string primaryButtonText = "OK",
        string? secondaryButtonText = null,
        string windowTitle = "NebulaDialog Success")
    {
        return ShowModal(
            owner,
            title,
            message,
            NebulaDialogVariant.Success,
            primaryButtonText,
            secondaryButtonText,
            null,
            windowTitle);
    }

    public static NebulaDialogResult ShowWarning(
        Window? owner,
        string title,
        string message,
        string primaryButtonText = "OK",
        string? secondaryButtonText = "Cancel",
        string? tertiaryButtonText = null,
        string windowTitle = "NebulaDialog Warning")
    {
        return ShowModal(
            owner,
            title,
            message,
            NebulaDialogVariant.Warning,
            primaryButtonText,
            secondaryButtonText,
            tertiaryButtonText,
            windowTitle);
    }

    public static NebulaDialogResult ShowDanger(
        Window? owner,
        string title,
        string message,
        string primaryButtonText = "Delete",
        string? secondaryButtonText = "Cancel",
        string windowTitle = "NebulaDialog Danger")
    {
        return ShowModal(
            owner,
            title,
            message,
            NebulaDialogVariant.Danger,
            primaryButtonText,
            secondaryButtonText,
            null,
            windowTitle);
    }

    public static bool Confirm(
        Window? owner,
        string title,
        string message,
        NebulaDialogVariant variant = NebulaDialogVariant.Warning,
        string primaryButtonText = "OK",
        string secondaryButtonText = "Cancel",
        string windowTitle = "NebulaDialog Confirm")
    {
        return ShowModal(
            owner,
            title,
            message,
            variant,
            primaryButtonText,
            secondaryButtonText,
            null,
            windowTitle) == NebulaDialogResult.Primary;
    }

    public static NebulaDialogResult ShowContent(
        Window? owner,
        string title,
        object content,
        NebulaDialogVariant variant = NebulaDialogVariant.Info,
        string primaryButtonText = "OK",
        string? secondaryButtonText = "Cancel",
        string? tertiaryButtonText = null,
        string windowTitle = "NebulaDialog",
        double width = 520)
    {
        return ShowModal(
            owner,
            title,
            string.Empty,
            variant,
            primaryButtonText,
            secondaryButtonText,
            tertiaryButtonText,
            windowTitle,
            width,
            content);
    }

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

    private static void OnDialogContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaDialog dialog && dialog.IsLoaded)
        {
            dialog.UpdateDialogContentVisibility();
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

    private void UpdateDialogContentVisibility()
    {
        var hasContent = DialogContent is not null;
        DialogContentPresenter.Visibility = hasContent ? Visibility.Visible : Visibility.Collapsed;
        MessageText.Visibility = string.IsNullOrWhiteSpace(Message) && hasContent
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
        DialogTitleText.Foreground = brush;
        PrimaryButton.Style = (Style)FindResource(GetPrimaryButtonStyleKey());

        InfoIconText.Visibility = Visibility.Collapsed;
        IconPath.Visibility = Visibility.Visible;

        IconPath.Data = Variant switch
        {
            NebulaDialogVariant.Success => Geometry.Parse("M 2 9 L 7 14 L 16 4"),
            NebulaDialogVariant.Warning => Geometry.Parse("M 9 2 L 16 15 H 2 Z M 9 6 V 10 M 9 13 H 9.1"),
            NebulaDialogVariant.Danger => Geometry.Parse("M 4 4 L 14 14 M 14 4 L 4 14"),
            _ => null
        };

        if (Variant is NebulaDialogVariant.Info or NebulaDialogVariant.Question)
        {
            InfoIconText.Text = Variant == NebulaDialogVariant.Question ? "?" : "i";
            IconPath.Visibility = Visibility.Collapsed;
            InfoIconText.Visibility = Visibility.Visible;
        }
    }

    private string GetPrimaryButtonStyleKey()
    {
        return Variant switch
        {
            NebulaDialogVariant.Success => "NebulaSuccessButton",
            NebulaDialogVariant.Warning => "NebulaWarningButton",
            NebulaDialogVariant.Danger => "NebulaDangerButton",
            _ => "NebulaPrimaryButton"
        };
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void NebulaDialog_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CloseWithResult(NebulaDialogResult.Close, false);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter && PrimaryButton.Visibility == Visibility.Visible)
        {
            CloseWithResult(NebulaDialogResult.Primary, true);
            e.Handled = true;
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
