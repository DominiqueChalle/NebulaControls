using System.Collections.ObjectModel;
using System.Windows;
using NebulaControls.Controls;
using NebulaControls.Theming;

namespace NebulaControls.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Components = new ObservableCollection<ComponentRow>
        {
            new("NebulaButtons", "Validated", "Actions"),
            new("NebulaTextBox", "Validated", "Inputs"),
            new("NebulaListBox", "Validated", "Selection"),
            new("NebulaDataGrid", "Reviewing", "Data"),
            new("NebulaDialog", "Validated", "Feedback")
        };

        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<ComponentRow> Components { get; }

    private void ApplyDarkPurpleThemeButton_Click(object sender, RoutedEventArgs e)
    {
        NebulaThemeManager.ApplyTheme(NebulaTheme.NebulaDarkPurple);
    }

    private void ApplyDarkBlueThemeButton_Click(object sender, RoutedEventArgs e)
    {
        NebulaThemeManager.ApplyTheme(NebulaTheme.NebulaDarkBlue);
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

    private void ShowDialog(string title, string message, NebulaDialogVariant variant)
    {
        var dialog = new NebulaDialog
        {
            Owner = this,
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

public sealed record ComponentRow(string Component, string Status, string Category);
