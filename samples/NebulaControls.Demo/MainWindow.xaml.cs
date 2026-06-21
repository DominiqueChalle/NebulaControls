using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NebulaControls.Demo.Views;
using NebulaControls.Theming;

namespace NebulaControls.Demo;

public partial class MainWindow : Window
{
    private Rect restoreBoundsBeforeMaximize;
    private bool isCustomMaximized;
    private bool isSidebarCollapsed;
    private readonly ButtonsFeedbackView buttonsFeedbackView = new();
    private readonly InputsProgressView inputsProgressView = new();
    private readonly CollectionsDataView collectionsDataView = new();
    private readonly ContainersLayoutView containersLayoutView = new();

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
        ApplyInitialWindowSize();
        UpdateSidebarLayout();
        ShowDemoView(buttonsFeedbackView, ActionsNavButton);
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

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleWindowState();
            return;
        }

        if (isCustomMaximized)
        {
            RestoreCustomMaximizedWindow();
        }

        DragMove();
    }

    private void MinimizeWindowButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeWindowButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ToggleWindowState()
    {
        if (isCustomMaximized)
        {
            RestoreCustomMaximizedWindow();
            return;
        }

        MaximizeToWorkArea();
    }

    private void MaximizeToWorkArea()
    {
        restoreBoundsBeforeMaximize = new Rect(Left, Top, Width, Height);

        var workArea = SystemParameters.WorkArea;

        WindowState = WindowState.Normal;
        Left = workArea.Left;
        Top = workArea.Top;
        Width = workArea.Width;
        Height = workArea.Height;

        isCustomMaximized = true;

        if (DemoWindowFrame is not null)
        {
            DemoWindowFrame.Margin = new Thickness(0);
            DemoWindowFrame.CornerRadius = new CornerRadius(0);
        }
    }

    private void RestoreCustomMaximizedWindow()
    {
        if (restoreBoundsBeforeMaximize.Width > 0 && restoreBoundsBeforeMaximize.Height > 0)
        {
            Left = restoreBoundsBeforeMaximize.Left;
            Top = restoreBoundsBeforeMaximize.Top;
            Width = restoreBoundsBeforeMaximize.Width;
            Height = restoreBoundsBeforeMaximize.Height;
        }

        isCustomMaximized = false;

        if (DemoWindowFrame is not null)
        {
            DemoWindowFrame.Margin = new Thickness(0);
            DemoWindowFrame.CornerRadius = new CornerRadius(10);
        }
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
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

    private void ActionsNavButton_Click(object sender, RoutedEventArgs e)
    {
        ShowDemoView(buttonsFeedbackView, ActionsNavButton);
    }

    private void InputsNavButton_Click(object sender, RoutedEventArgs e)
    {
        ShowDemoView(inputsProgressView, InputsNavButton);
    }

    private void CollectionsNavButton_Click(object sender, RoutedEventArgs e)
    {
        ShowDemoView(collectionsDataView, CollectionsNavButton);
    }

    private void ContainersNavButton_Click(object sender, RoutedEventArgs e)
    {
        ShowDemoView(containersLayoutView, ContainersNavButton);
    }

    private void SidebarToggleButton_Click(object sender, RoutedEventArgs e)
    {
        isSidebarCollapsed = !isSidebarCollapsed;
        UpdateSidebarLayout();
    }

    private void ShowDemoView(UserControl selectedView, Button selectedButton)
    {
        Button[] buttons =
        [
            ActionsNavButton,
            InputsNavButton,
            CollectionsNavButton,
            ContainersNavButton
        ];

        selectedView.DataContext = this;
        DemoContentHost.Content = selectedView;

        foreach (var button in buttons)
        {
            button.ClearValue(BackgroundProperty);
            button.ClearValue(BorderBrushProperty);
            button.ClearValue(ForegroundProperty);
        }

        selectedButton.SetResourceReference(BackgroundProperty, "Brush.SurfaceActive");
        selectedButton.SetResourceReference(BorderBrushProperty, "Brush.BorderFocus");
        selectedButton.SetResourceReference(ForegroundProperty, "Brush.TextPrimary");
    }

    private void UpdateSidebarLayout()
    {
        SidebarColumn.Width = new GridLength(isSidebarCollapsed ? 74 : 244);
        SidebarPanel.Padding = isSidebarCollapsed
            ? new Thickness(12, 20, 12, 24)
            : new Thickness(24, 20, 18, 24);

        SidebarTitleText.Visibility = isSidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
        SidebarDescriptionText.Visibility = isSidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
        SidebarToggleButton.Content = isSidebarCollapsed ? ">" : "<";
        SidebarToggleButton.HorizontalAlignment = isSidebarCollapsed
            ? HorizontalAlignment.Center
            : HorizontalAlignment.Right;

        UpdateSidebarButton(ActionsNavButton, "Buttons + Feedback", "\uE8FD");
        UpdateSidebarButton(InputsNavButton, "Inputs + Progress", "\uE70F");
        UpdateSidebarButton(CollectionsNavButton, "Collections + Data", "\uE8A5");
        UpdateSidebarButton(ContainersNavButton, "Containers + Layout", "\uE8A9");
    }

    private void UpdateSidebarButton(Button button, string label, string iconGlyph)
    {
        button.Content = isSidebarCollapsed
            ? CreateSidebarIcon(iconGlyph)
            : CreateSidebarLabel(iconGlyph, label);
        button.ToolTip = label;
        button.HorizontalContentAlignment = isSidebarCollapsed
            ? HorizontalAlignment.Center
            : HorizontalAlignment.Left;
    }

    private static TextBlock CreateSidebarIcon(string iconGlyph)
    {
        return new TextBlock
        {
            Text = iconGlyph,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 15,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private static StackPanel CreateSidebarLabel(string iconGlyph, string label)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock
                {
                    Text = iconGlyph,
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    FontSize = 14,
                    Width = 22,
                    VerticalAlignment = VerticalAlignment.Center
                },
                new TextBlock
                {
                    Text = label,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
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
}

public sealed record ComponentRow(string Component, string Status, string Category);
