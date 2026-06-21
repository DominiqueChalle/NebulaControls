using System.Windows;
using System.Windows.Controls;
using NebulaControls.Controls;

namespace NebulaControls.Demo.Views;

public partial class ButtonsFeedbackView : UserControl
{
    public ButtonsFeedbackView()
    {
        InitializeComponent();
    }

    private void ShowInfoDialogButton_Click(object sender, RoutedEventArgs e)
    {
        ShowDialog("Information", "NebulaDialog is loaded from the external package.", NebulaDialogVariant.Info);
    }

    private void ShowWarningDialogButton_Click(object sender, RoutedEventArgs e)
    {
        ShowDialog("Warning", "This is the warning variant with the same classic dialog behavior.", NebulaDialogVariant.Warning);
    }

    private void ShowDangerDialogButton_Click(object sender, RoutedEventArgs e)
    {
        ShowDialog("Danger", "This is the danger variant. The rail and icon should reflect the severity.", NebulaDialogVariant.Danger);
    }

    private void ShowInfoAlertButton_Click(object sender, RoutedEventArgs e)
    {
        ShowAlert("NebulaInfoAlert", "Information", "Use inline alerts to explain a state without interrupting the user.");
    }

    private void ShowSuccessAlertButton_Click(object sender, RoutedEventArgs e)
    {
        ShowAlert("NebulaSuccessAlert", "Saved successfully", "The current settings were applied.");
    }

    private void ShowWarningAlertButton_Click(object sender, RoutedEventArgs e)
    {
        ShowAlert("NebulaWarningAlert", "Check required fields", "Some values may need attention before continuing.");
    }

    private void ShowDangerAlertButton_Click(object sender, RoutedEventArgs e)
    {
        ShowAlert("NebulaDangerAlert", "Action failed", "The operation could not be completed.");
    }

    private void ShowAlert(string styleKey, string title, string message)
    {
        InteractiveAlert.Style = (Style)FindResource(styleKey);
        InteractiveAlertTitle.Text = title;
        InteractiveAlertMessage.Text = message;
    }

    private void ShowDialog(string title, string message, NebulaDialogVariant variant)
    {
        var dialog = new NebulaDialog
        {
            Owner = Window.GetWindow(this),
            Title = $"NebulaDialog {variant}",
            DialogTitle = title,
            Message = message,
            Variant = variant,
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel"
        };

        dialog.ShowDialog();
    }
}
