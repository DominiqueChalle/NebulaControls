using System.Windows.Controls;
using NebulaControls.Controls;

namespace NebulaControls.Demo.Views;

public partial class InputsProgressView : UserControl
{
    public InputsProgressView()
    {
        InitializeComponent();
    }

    private void EmailTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is not NebulaTextBox textBox)
        {
            return;
        }

        var email = textBox.Text.Trim();
        ClearEmailValidation(textBox);

        if (string.IsNullOrWhiteSpace(email))
        {
            textBox.HasError = true;
            textBox.ErrorText = "Email is required.";
            return;
        }

        if (!IsValidEmail(email))
        {
            textBox.HasWarning = true;
            textBox.WarningText = "Email looks incomplete or invalid.";
        }
    }

    private static void ClearEmailValidation(NebulaTextBox textBox)
    {
        textBox.HasError = false;
        textBox.HasWarning = false;
        textBox.ErrorText = string.Empty;
        textBox.WarningText = string.Empty;
    }

    private static bool IsValidEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        var dotIndex = email.LastIndexOf('.');

        return atIndex > 0
            && dotIndex > atIndex + 1
            && dotIndex < email.Length - 1;
    }

    private void PasswordValidationBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is not NebulaLabeledPasswordBox passwordBox)
        {
            return;
        }

        var password = passwordBox.Password;
        ClearPasswordValidation(passwordBox);

        if (string.IsNullOrWhiteSpace(password))
        {
            passwordBox.HasError = true;
            passwordBox.ErrorText = "Password is required.";
            return;
        }

        if (!IsStrongPassword(password))
        {
            passwordBox.HasWarning = true;
            passwordBox.WarningText = "Use at least 8 characters and one special character.";
        }
    }

    private static void ClearPasswordValidation(NebulaLabeledPasswordBox passwordBox)
    {
        passwordBox.HasError = false;
        passwordBox.HasWarning = false;
        passwordBox.ErrorText = string.Empty;
        passwordBox.WarningText = string.Empty;
    }

    private static bool IsStrongPassword(string password)
    {
        return password.Length >= 8
            && password.Any(character => !char.IsLetterOrDigit(character));
    }
}
