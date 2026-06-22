using System.Windows;
using System.Windows.Controls;
using NebulaControls.Controls;

namespace NebulaControls.Demo.Views;

public partial class ButtonsFeedbackView : UserControl
{
    private readonly ChipDemoItem[] chipDemoItems =
    [
        new("NebulaTextBox", "Inputs"),
        new("NebulaSearchBox", "Inputs"),
        new("NebulaNumericUpDown", "Inputs"),
        new("NebulaAlert", "Feedback"),
        new("NebulaRating", "Feedback"),
        new("NebulaChip", "Feedback"),
        new("NebulaAvatar", "New controls"),
        new("NebulaDatePicker", "New controls")
    ];

    public ButtonsFeedbackView()
    {
        InitializeComponent();
        ApplyChipFilters();
    }

    public event EventHandler<ToastRequestedEventArgs>? ToastRequested;

    private void ShowInfoDialogButton_Click(object sender, RoutedEventArgs e)
    {
        var result = NebulaDialog.ShowModal(
            Window.GetWindow(this),
            "Information",
            "NebulaDialog can show a simple modal message with one action.",
            NebulaDialogVariant.Info,
            "OK",
            null,
            windowTitle: "NebulaDialog Info");

        UpdateDialogResult("Information", result);
    }

    private void ShowSaveDialogButton_Click(object sender, RoutedEventArgs e)
    {
        var result = NebulaDialog.ShowModal(
            Window.GetWindow(this),
            "Save changes?",
            "The current profile has local changes. Save them before continuing?",
            NebulaDialogVariant.Success,
            "Save",
            "Cancel",
            windowTitle: "NebulaDialog Save");

        UpdateDialogResult("Save", result);
    }

    private void ShowDeleteDialogButton_Click(object sender, RoutedEventArgs e)
    {
        var result = NebulaDialog.ShowModal(
            Window.GetWindow(this),
            "Delete selected item?",
            "This action removes the selected item from the local demo list.",
            NebulaDialogVariant.Danger,
            "Delete",
            "Cancel",
            windowTitle: "NebulaDialog Delete");

        UpdateDialogResult("Delete", result);
    }

    private void ShowUnsavedDialogButton_Click(object sender, RoutedEventArgs e)
    {
        var result = NebulaDialog.ShowModal(
            Window.GetWindow(this),
            "Unsaved changes",
            "You can save your changes, discard them, or return to the demo without closing anything.",
            NebulaDialogVariant.Warning,
            "Save",
            "Cancel",
            "Discard",
            "NebulaDialog Unsaved");

        UpdateDialogResult("Unsaved", result);
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

    private void ShowInfoToastButton_Click(object sender, RoutedEventArgs e)
    {
        ShowToast("NebulaInfoToast", "Information", "NebulaControls demo is running with the current theme.");
    }

    private void ShowSuccessToastButton_Click(object sender, RoutedEventArgs e)
    {
        ShowToast("NebulaSuccessToast", "File saved", "Your local settings were saved successfully.");
    }

    private void ShowWarningToastButton_Click(object sender, RoutedEventArgs e)
    {
        ShowToast("NebulaWarningToast", "Review suggested", "Some optional fields are still empty.");
    }

    private void ShowDangerToastButton_Click(object sender, RoutedEventArgs e)
    {
        ShowToast("NebulaDangerToast", "Sync failed", "The remote server could not be reached.");
    }

    private void FeedbackRating_ValueChanged(object sender, System.EventArgs e)
    {
        if (sender is not NebulaRating rating)
        {
            return;
        }

        if (FeedbackRatingText is null)
        {
            return;
        }

        FeedbackRatingText.Text = rating.Value switch
        {
            0 => "No rating selected.",
            1 => "1 / 5 - Needs work",
            2 => "2 / 5 - Not quite there",
            3 => "3 / 5 - Good baseline",
            4 => "4 / 5 - Very useful",
            _ => "5 / 5 - Excellent"
        };
    }

    private void FilterChip_CloseClicked(object sender, System.EventArgs e)
    {
        if (sender is NebulaChip chip)
        {
            chip.IsSelected = false;
            chip.Visibility = Visibility.Collapsed;
            ApplyChipFilters();
        }
    }

    private void FilterChip_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ApplyChipFilters();
    }

    private void AddFilterChip_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not NebulaChip { Tag: string category })
        {
            return;
        }

        var chip = category switch
        {
            "Inputs" => InputFilterChip,
            "Feedback" => FeedbackFilterChip,
            "New controls" => NewFilterChip,
            _ => null
        };

        if (chip is null)
        {
            return;
        }

        chip.Visibility = Visibility.Visible;
        chip.IsSelected = true;
        ApplyChipFilters();
    }

    private void ApplyChipFilters()
    {
        var activeCategories = new List<string>();

        AddCategoryIfActive(InputFilterChip, activeCategories);
        AddCategoryIfActive(FeedbackFilterChip, activeCategories);
        AddCategoryIfActive(NewFilterChip, activeCategories);

        var filteredItems = activeCategories.Count == 0
            ? chipDemoItems
            : chipDemoItems.Where(item => activeCategories.Contains(item.Category)).ToArray();

        ChipFilteredControlsListBox.ItemsSource = filteredItems;

        FilterStatusText.Text = activeCategories.Count == 0
            ? $"No active chip filter. Showing all {filteredItems.Length} controls."
            : $"{filteredItems.Length} control(s) match {activeCategories.Count} active chip filter(s).";
    }

    private static void AddCategoryIfActive(NebulaChip chip, ICollection<string> activeCategories)
    {
        if (chip.Visibility == Visibility.Visible && chip.IsSelected && chip.Content is string category)
        {
            activeCategories.Add(category);
        }
    }

    private void ShowAlert(string styleKey, string title, string message)
    {
        InteractiveAlert.Style = (Style)FindResource(styleKey);
        InteractiveAlertTitle.Text = title;
        InteractiveAlertMessage.Text = message;
    }

    private void ShowToast(string styleKey, string title, string message)
    {
        ToastRequested?.Invoke(this, new ToastRequestedEventArgs(styleKey, title, message));
    }

    private void UpdateDialogResult(string scenario, NebulaDialogResult result)
    {
        DialogResultText.Text = $"{scenario} dialog result: {result}";
    }

    private sealed record ChipDemoItem(string Name, string Category);
}

public sealed class ToastRequestedEventArgs(string styleKey, string title, string message) : EventArgs
{
    public string StyleKey { get; } = styleKey;

    public string Title { get; } = title;

    public string Message { get; } = message;
}
