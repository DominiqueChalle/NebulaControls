// Nom: NebulaLabeledPasswordBox
// Version: V1.03
// Description: Labeled password input control with helper text, reveal support and configurable password rule validation.

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

    public static readonly DependencyProperty PasswordRuleProperty =
        DependencyProperty.Register(
            nameof(PasswordRule),
            typeof(string),
            typeof(NebulaLabeledPasswordBox),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsPasswordRequiredProperty =
        DependencyProperty.Register(
            nameof(IsPasswordRequired),
            typeof(bool),
            typeof(NebulaLabeledPasswordBox),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ValidateOnLostFocusProperty =
        DependencyProperty.Register(
            nameof(ValidateOnLostFocus),
            typeof(bool),
            typeof(NebulaLabeledPasswordBox),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ValidateOnPasswordChangedProperty =
        DependencyProperty.Register(
            nameof(ValidateOnPasswordChanged),
            typeof(bool),
            typeof(NebulaLabeledPasswordBox),
            new PropertyMetadata(false));

    public static readonly DependencyProperty RequiredPasswordTextProperty =
        DependencyProperty.Register(
            nameof(RequiredPasswordText),
            typeof(string),
            typeof(NebulaLabeledPasswordBox),
            new PropertyMetadata("Password is required."));

    public static readonly DependencyProperty InvalidPasswordTextProperty =
        DependencyProperty.Register(
            nameof(InvalidPasswordText),
            typeof(string),
            typeof(NebulaLabeledPasswordBox),
            new PropertyMetadata("Password does not match the required format."));

    public static readonly DependencyProperty IsPasswordValidProperty =
        DependencyProperty.Register(
            nameof(IsPasswordValid),
            typeof(bool),
            typeof(NebulaLabeledPasswordBox),
            new PropertyMetadata(false));

    public bool IsPasswordVisible
    {
        get => (bool)GetValue(IsPasswordVisibleProperty);
        set => SetValue(IsPasswordVisibleProperty, value);
    }

    public string PasswordRule
    {
        get => (string)GetValue(PasswordRuleProperty);
        set => SetValue(PasswordRuleProperty, value);
    }

    public bool IsPasswordRequired
    {
        get => (bool)GetValue(IsPasswordRequiredProperty);
        set => SetValue(IsPasswordRequiredProperty, value);
    }

    public bool ValidateOnLostFocus
    {
        get => (bool)GetValue(ValidateOnLostFocusProperty);
        set => SetValue(ValidateOnLostFocusProperty, value);
    }

    public bool ValidateOnPasswordChanged
    {
        get => (bool)GetValue(ValidateOnPasswordChangedProperty);
        set => SetValue(ValidateOnPasswordChangedProperty, value);
    }

    public string RequiredPasswordText
    {
        get => (string)GetValue(RequiredPasswordTextProperty);
        set => SetValue(RequiredPasswordTextProperty, value);
    }

    public string InvalidPasswordText
    {
        get => (string)GetValue(InvalidPasswordTextProperty);
        set => SetValue(InvalidPasswordTextProperty, value);
    }

    public bool IsPasswordValid
    {
        get => (bool)GetValue(IsPasswordValidProperty);
        private set => SetValue(IsPasswordValidProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (passwordBox is not null)
        {
            passwordBox.PasswordChanged -= OnInnerPasswordChanged;
            passwordBox.LostFocus -= OnInnerPasswordLostFocus;
        }

        if (revealedTextBox is not null)
        {
            revealedTextBox.TextChanged -= OnRevealedTextChanged;
            revealedTextBox.LostFocus -= OnRevealedTextLostFocus;
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
            passwordBox.LostFocus += OnInnerPasswordLostFocus;
        }

        if (revealedTextBox is not null)
        {
            revealedTextBox.Text = Password;
            revealedTextBox.TextChanged += OnRevealedTextChanged;
            revealedTextBox.LostFocus += OnRevealedTextLostFocus;
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

        ValidatePasswordIfRequested();
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

        ValidatePasswordIfRequested();
    }

    private void OnRevealButtonClick(object sender, RoutedEventArgs e)
    {
        if (!IsPasswordRevealEnabled)
        {
            return;
        }

        IsPasswordVisible = !IsPasswordVisible;
    }

    private void OnInnerPasswordLostFocus(object sender, RoutedEventArgs e)
    {
        ValidatePasswordOnLostFocus();
    }

    private void OnRevealedTextLostFocus(object sender, RoutedEventArgs e)
    {
        ValidatePasswordOnLostFocus();
    }

    public void ValidatePassword()
    {
        ClearPasswordValidation();

        if (string.IsNullOrWhiteSpace(Password))
        {
            IsPasswordValid = !IsPasswordRequired;
            if (IsPasswordRequired)
            {
                HasError = true;
                ErrorText = RequiredPasswordText;
            }

            return;
        }

        if (!MatchesPasswordRule(Password, PasswordRule))
        {
            IsPasswordValid = false;
            HasWarning = true;
            WarningText = InvalidPasswordText;
            return;
        }

        IsPasswordValid = true;
    }

    private void ValidatePasswordIfRequested()
    {
        if (ValidateOnPasswordChanged)
        {
            ValidatePassword();
        }
    }

    private void ValidatePasswordOnLostFocus()
    {
        if (ValidateOnLostFocus)
        {
            ValidatePassword();
        }
    }

    private void ClearPasswordValidation()
    {
        HasError = false;
        HasWarning = false;
        ErrorText = string.Empty;
        WarningText = string.Empty;
        IsPasswordValid = false;
    }

    private static bool MatchesPasswordRule(string password, string passwordRule)
    {
        if (string.IsNullOrWhiteSpace(passwordRule))
        {
            return true;
        }

        var tokens = passwordRule.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var token in tokens)
        {
            if (!MatchesPasswordRuleToken(password, token))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesPasswordRuleToken(string password, string token)
    {
        if (token.StartsWith("min:", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(token[4..], out var minLength) && password.Length >= minLength;
        }

        if (token.StartsWith("max:", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(token[4..], out var maxLength) && password.Length <= maxLength;
        }

        return token.ToLowerInvariant() switch
        {
            "upper" => password.Any(char.IsUpper),
            "lower" => password.Any(char.IsLower),
            "digit" => password.Any(char.IsDigit),
            "special" => password.Any(character => !char.IsLetterOrDigit(character)),
            "letters" => password.All(char.IsLetter),
            "alnum" => password.All(char.IsLetterOrDigit),
            "lowerdigit" => password.All(character => char.IsLower(character) || char.IsDigit(character))
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit),
            _ => true
        };
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
