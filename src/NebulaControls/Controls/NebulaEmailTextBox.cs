// Nom: NebulaEmailTextBox
// Version: V1.00
// Description: Specialized NebulaTextBox with built-in email format validation.

using System.Windows;

namespace NebulaControls.Controls;

public class NebulaEmailTextBox : NebulaTextBox
{
    public static readonly DependencyProperty IsEmailRequiredProperty =
        DependencyProperty.Register(
            nameof(IsEmailRequired),
            typeof(bool),
            typeof(NebulaEmailTextBox),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ValidateOnLostFocusProperty =
        DependencyProperty.Register(
            nameof(ValidateOnLostFocus),
            typeof(bool),
            typeof(NebulaEmailTextBox),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ValidateOnTextChangedProperty =
        DependencyProperty.Register(
            nameof(ValidateOnTextChanged),
            typeof(bool),
            typeof(NebulaEmailTextBox),
            new PropertyMetadata(false));

    public static readonly DependencyProperty RequiredEmailTextProperty =
        DependencyProperty.Register(
            nameof(RequiredEmailText),
            typeof(string),
            typeof(NebulaEmailTextBox),
            new PropertyMetadata("Email is required."));

    public static readonly DependencyProperty InvalidEmailTextProperty =
        DependencyProperty.Register(
            nameof(InvalidEmailText),
            typeof(string),
            typeof(NebulaEmailTextBox),
            new PropertyMetadata("Email looks incomplete or invalid."));

    public static readonly DependencyProperty IsEmailValidProperty =
        DependencyProperty.Register(
            nameof(IsEmailValid),
            typeof(bool),
            typeof(NebulaEmailTextBox),
            new PropertyMetadata(false));

    public bool IsEmailRequired
    {
        get => (bool)GetValue(IsEmailRequiredProperty);
        set => SetValue(IsEmailRequiredProperty, value);
    }

    public bool ValidateOnLostFocus
    {
        get => (bool)GetValue(ValidateOnLostFocusProperty);
        set => SetValue(ValidateOnLostFocusProperty, value);
    }

    public bool ValidateOnTextChanged
    {
        get => (bool)GetValue(ValidateOnTextChangedProperty);
        set => SetValue(ValidateOnTextChangedProperty, value);
    }

    public string RequiredEmailText
    {
        get => (string)GetValue(RequiredEmailTextProperty);
        set => SetValue(RequiredEmailTextProperty, value);
    }

    public string InvalidEmailText
    {
        get => (string)GetValue(InvalidEmailTextProperty);
        set => SetValue(InvalidEmailTextProperty, value);
    }

    public bool IsEmailValid
    {
        get => (bool)GetValue(IsEmailValidProperty);
        private set => SetValue(IsEmailValidProperty, value);
    }

    public void ValidateEmail()
    {
        var email = Text.Trim();
        ClearValidation();

        if (string.IsNullOrWhiteSpace(email))
        {
            IsEmailValid = !IsEmailRequired;
            if (IsEmailRequired)
            {
                HasError = true;
                ErrorText = RequiredEmailText;
            }

            return;
        }

        if (!IsValidEmailFormat(email))
        {
            IsEmailValid = false;
            HasWarning = true;
            WarningText = InvalidEmailText;
            return;
        }

        IsEmailValid = true;
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);

        if (ValidateOnLostFocus)
        {
            ValidateEmail();
        }
    }

    protected override void OnTextChanged(System.Windows.Controls.TextChangedEventArgs e)
    {
        base.OnTextChanged(e);

        if (ValidateOnTextChanged)
        {
            ValidateEmail();
        }
    }

    private void ClearValidation()
    {
        HasError = false;
        HasWarning = false;
        ErrorText = string.Empty;
        WarningText = string.Empty;
        IsEmailValid = false;
    }

    private static bool IsValidEmailFormat(string email)
    {
        if (email.Any(char.IsWhiteSpace))
        {
            return false;
        }

        var parts = email.Split('@');
        if (parts.Length != 2)
        {
            return false;
        }

        var localPart = parts[0];
        var domainPart = parts[1];
        if (string.IsNullOrWhiteSpace(localPart)
            || string.IsNullOrWhiteSpace(domainPart)
            || localPart.StartsWith('.')
            || localPart.EndsWith('.'))
        {
            return false;
        }

        var domainSegments = domainPart.Split('.');
        return domainSegments.Length >= 2
            && domainSegments.All(segment => !string.IsNullOrWhiteSpace(segment));
    }
}
