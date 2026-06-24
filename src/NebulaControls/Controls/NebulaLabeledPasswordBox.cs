// Nom: NebulaLabeledPasswordBox
// Version: V1.02
// Description: Labeled password input control with helper text and validation state support.

using System.Windows;
using System.Windows.Controls;

namespace NebulaControls.Controls;

public class NebulaLabeledPasswordBox : Control
{
    private PasswordBox? passwordBox;
    private TextBox? revealedTextBox;
    private Button? revealButton;
    private bool isUpdatingPassword;

    static NebulaLabeledPasswordBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaLabeledPasswordBox),
            new FrameworkPropertyMetadata(typeof(NebulaLabeledPasswordBox)));
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(NebulaLabeledPasswordBox),
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
            typeof(NebulaLabeledPasswordBox),
            new PropertyMetadata(Dock.Top));

    public Dock LabelPlacement
    {
        get => (Dock)GetValue(LabelPlacementProperty);
        set => SetValue(LabelPlacementProperty, value);
    }

    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register(
            nameof(Password),
            typeof(string),
            typeof(NebulaLabeledPasswordBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));

    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public static readonly DependencyProperty HelperTextProperty =
        DependencyProperty.Register(
            nameof(HelperText),
            typeof(string),
            typeof(NebulaLabeledPasswordBox),
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
            typeof(NebulaLabeledPasswordBox),
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
            typeof(NebulaLabeledPasswordBox),
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
            typeof(NebulaLabeledPasswordBox),
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
            typeof(NebulaLabeledPasswordBox),
            new PropertyMetadata(false));

    public bool HasWarning
    {
        get => (bool)GetValue(HasWarningProperty);
        set => SetValue(HasWarningProperty, value);
    }

    public static readonly DependencyProperty IsPasswordRevealEnabledProperty =
        DependencyProperty.Register(
            nameof(IsPasswordRevealEnabled),
            typeof(bool),
            typeof(NebulaLabeledPasswordBox),
            new PropertyMetadata(true, OnPasswordRevealEnabledChanged));

    public bool IsPasswordRevealEnabled
    {
        get => (bool)GetValue(IsPasswordRevealEnabledProperty);
        set => SetValue(IsPasswordRevealEnabledProperty, value);
    }

    public static readonly DependencyProperty IsPasswordVisibleProperty =
        DependencyProperty.Register(
            nameof(IsPasswordVisible),
            typeof(bool),
            typeof(NebulaLabeledPasswordBox),
            new PropertyMetadata(false, OnPasswordVisibilityChanged));

    public bool IsPasswordVisible
    {
        get => (bool)GetValue(IsPasswordVisibleProperty);
        set => SetValue(IsPasswordVisibleProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (passwordBox is not null)
        {
            passwordBox.PasswordChanged -= OnInnerPasswordChanged;
        }

        if (revealedTextBox is not null)
        {
            revealedTextBox.TextChanged -= OnRevealedTextChanged;
        }

        if (revealButton is not null)
        {
            revealButton.Click -= OnRevealButtonClick;
        }

        base.OnApplyTemplate();

        passwordBox = GetTemplateChild("PART_PasswordBox") as PasswordBox;
        revealedTextBox = GetTemplateChild("PART_RevealedTextBox") as TextBox;
        revealButton = GetTemplateChild("PART_RevealButton") as Button;

        if (passwordBox is not null)
        {
            passwordBox.Password = Password;
            passwordBox.PasswordChanged += OnInnerPasswordChanged;
        }

        if (revealedTextBox is not null)
        {
            revealedTextBox.Text = Password;
            revealedTextBox.TextChanged += OnRevealedTextChanged;
        }

        if (revealButton is not null)
        {
            revealButton.Click += OnRevealButtonClick;
        }

        UpdateRevealState();
    }

    private static void OnPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var control = (NebulaLabeledPasswordBox)dependencyObject;

        if (control.isUpdatingPassword)
        {
            return;
        }

        var newPassword = e.NewValue as string ?? string.Empty;

        if (control.passwordBox is not null && control.passwordBox.Password != newPassword)
        {
            control.passwordBox.Password = newPassword;
        }

        if (control.revealedTextBox is not null && control.revealedTextBox.Text != newPassword)
        {
            control.revealedTextBox.Text = newPassword;
        }
    }

    private static void OnPasswordRevealEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var control = (NebulaLabeledPasswordBox)dependencyObject;

        if (!control.IsPasswordRevealEnabled)
        {
            control.IsPasswordVisible = false;
        }

        control.UpdateRevealState();
    }

    private static void OnPasswordVisibilityChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var control = (NebulaLabeledPasswordBox)dependencyObject;
        control.UpdateRevealState();
    }

    private void OnInnerPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (passwordBox is null)
        {
            return;
        }

        isUpdatingPassword = true;
        Password = passwordBox.Password;
        if (revealedTextBox is not null && revealedTextBox.Text != passwordBox.Password)
        {
            revealedTextBox.Text = passwordBox.Password;
        }

        isUpdatingPassword = false;
    }

    private void OnRevealedTextChanged(object sender, TextChangedEventArgs e)
    {
        if (revealedTextBox is null)
        {
            return;
        }

        isUpdatingPassword = true;
        Password = revealedTextBox.Text;
        if (passwordBox is not null && passwordBox.Password != revealedTextBox.Text)
        {
            passwordBox.Password = revealedTextBox.Text;
        }

        isUpdatingPassword = false;
    }

    private void OnRevealButtonClick(object sender, RoutedEventArgs e)
    {
        if (!IsPasswordRevealEnabled)
        {
            return;
        }

        IsPasswordVisible = !IsPasswordVisible;
    }

    private void UpdateRevealState()
    {
        var showRevealedText = IsPasswordRevealEnabled && IsPasswordVisible;

        if (passwordBox is not null)
        {
            passwordBox.Visibility = showRevealedText ? Visibility.Collapsed : Visibility.Visible;
        }

        if (revealedTextBox is not null)
        {
            revealedTextBox.Visibility = showRevealedText ? Visibility.Visible : Visibility.Collapsed;
        }

        if (revealButton is not null)
        {
            revealButton.Visibility = IsPasswordRevealEnabled ? Visibility.Visible : Visibility.Collapsed;
            revealButton.Content = showRevealedText ? "Hide" : "Show";
        }
    }
}
