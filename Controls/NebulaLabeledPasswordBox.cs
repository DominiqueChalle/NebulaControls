using System.Windows;
using System.Windows.Controls;

namespace NebulaUI.Controls;

public class NebulaLabeledPasswordBox : Control
{
    private PasswordBox? passwordBox;
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

    public override void OnApplyTemplate()
    {
        if (passwordBox is not null)
        {
            passwordBox.PasswordChanged -= OnInnerPasswordChanged;
        }

        base.OnApplyTemplate();

        passwordBox = GetTemplateChild("PART_PasswordBox") as PasswordBox;

        if (passwordBox is not null)
        {
            passwordBox.Password = Password;
            passwordBox.PasswordChanged += OnInnerPasswordChanged;
        }
    }

    private static void OnPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var control = (NebulaLabeledPasswordBox)dependencyObject;

        if (control.passwordBox is null || control.isUpdatingPassword)
        {
            return;
        }

        var newPassword = e.NewValue as string ?? string.Empty;

        if (control.passwordBox.Password != newPassword)
        {
            control.passwordBox.Password = newPassword;
        }
    }

    private void OnInnerPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (passwordBox is null)
        {
            return;
        }

        isUpdatingPassword = true;
        Password = passwordBox.Password;
        isUpdatingPassword = false;
    }
}
