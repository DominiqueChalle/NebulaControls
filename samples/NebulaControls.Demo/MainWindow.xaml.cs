using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NebulaControls.Controls;
using NebulaControls.Demo.Behaviors;
using NebulaControls.Demo.Views;
using NebulaControls.Theming;

namespace NebulaControls.Demo;

public partial class MainWindow : NebulaWindow
{
    private readonly ButtonsFeedbackView buttonsFeedbackView = new();
    private readonly InputsProgressView inputsProgressView = new();
    private readonly CollectionsDataView collectionsDataView = new();
    private readonly ControlInventoryView controlInventoryView = new();
    private readonly ContainersLayoutView containersLayoutView = new();

    public MainWindow()
    {
        Components = new ObservableCollection<ComponentRow>
        {
            new("NebulaButtons", "Validated", "Actions"),
            new("NebulaTextBox", "Validated", "Inputs"),
            new("NebulaListBox", "Validated", "Selection"),
            new("NebulaDataGrid", "Validated", "Data"),
            new("NebulaDialog", "Validated", "Feedback")
        };

        InitializeComponent();
        buttonsFeedbackView.ToastRequested += ButtonsFeedbackView_ToastRequested;
        DataContext = this;
        ApplyInitialWindowSize();
        DemoSidebar.SelectedIndex = 0;
        ShowDemoView(buttonsFeedbackView);
        UpdateThemeLabels(NebulaTheme.NebulaDarkPurple);
    }

    public ObservableCollection<ComponentRow> Components { get; }

    private void ApplyInitialWindowSize()
    {
        var workArea = SystemParameters.WorkArea;

        Width = Math.Max(MinWidth, workArea.Width * 0.8);
        Height = Math.Max(MinHeight, workArea.Height * 0.8);
        Left = workArea.Left + (workArea.Width - Width) / 2;
        Top = workArea.Top + (workArea.Height - Height) / 2;
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MenuCommand_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            ToastHost.ShowInfo("Menu command", $"{item.Header} selected.");
        }
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow
        {
            Owner = this
        };

        aboutWindow.ShowDialog();
    }

    private void DocumentationPreviewMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var documentationWindow = new DocumentationWindow
        {
            Owner = this
        };

        documentationWindow.Show();
    }

    private void ApplyDarkPurpleThemeMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ApplyTheme(NebulaTheme.NebulaDarkPurple);
    }

    private void ApplyDarkBlueThemeMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ApplyTheme(NebulaTheme.NebulaDarkBlue);
    }

    private void ApplyLightPurpleThemeMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ApplyTheme(NebulaTheme.NebulaLightPurple);
    }

    private void DemoSidebar_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DemoSidebar.SelectedItem is not NebulaSidebarItem { Tag: string key })
        {
            return;
        }

        ShowDemoView(GetViewForSidebarKey(key));
    }

    private UserControl GetViewForSidebarKey(string key)
    {
        return key switch
        {
            "Inputs" => inputsProgressView,
            "Collections" => collectionsDataView,
            "Inventory" => controlInventoryView,
            "Containers" => containersLayoutView,
            _ => buttonsFeedbackView
        };
    }

    private void ShowDemoView(UserControl selectedView)
    {
        if (DemoContentHost is null)
        {
            return;
        }

        selectedView.DataContext = this;
        DemoContentHost.Content = selectedView;

        Dispatcher.BeginInvoke(
            () => ScrollFocusBehavior.FocusFirstKeyboardTarget(selectedView),
            DispatcherPriority.ContextIdle);
    }

    private void ApplyTheme(NebulaTheme theme)
    {
        NebulaThemeManager.ApplyTheme(theme);
        UpdateThemeLabels(theme);
    }

    private void UpdateThemeLabels(NebulaTheme theme)
    {
        var themeName = theme.ToString();

        if (ActiveThemeStatusText is not null)
        {
            ActiveThemeStatusText.Text = $"{themeName} theme active";
        }

    }

    private void ButtonsFeedbackView_ToastRequested(object? sender, ToastRequestedEventArgs e)
    {
        ToastHost.Show(e.Variant, e.Title, e.Message);
    }
}

public sealed record ComponentRow(string Component, string Status, string Category);
