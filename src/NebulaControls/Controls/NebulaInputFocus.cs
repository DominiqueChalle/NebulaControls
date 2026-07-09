// Nom: NebulaInputFocus
// Version: V1.00
// Description: Shared input focus behavior for text and password controls.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public static class NebulaInputFocus
{
    public static readonly DependencyProperty SelectAllOnFocusProperty =
        DependencyProperty.RegisterAttached(
            "SelectAllOnFocus",
            typeof(bool),
            typeof(NebulaInputFocus),
            new PropertyMetadata(false, OnSelectAllOnFocusChanged));

    public static bool GetSelectAllOnFocus(DependencyObject obj)
    {
        return (bool)obj.GetValue(SelectAllOnFocusProperty);
    }

    public static void SetSelectAllOnFocus(DependencyObject obj, bool value)
    {
        obj.SetValue(SelectAllOnFocusProperty, value);
    }

    private static void OnSelectAllOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
                return;
            }

            textBox.GotKeyboardFocus -= TextBox_GotKeyboardFocus;
        }

        if (d is PasswordBox passwordBox)
        {
            if ((bool)e.NewValue)
            {
                passwordBox.GotKeyboardFocus += PasswordBox_GotKeyboardFocus;
                return;
            }

            passwordBox.GotKeyboardFocus -= PasswordBox_GotKeyboardFocus;
        }
    }

    private static void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    private static void PasswordBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            passwordBox.SelectAll();
        }
    }
}
