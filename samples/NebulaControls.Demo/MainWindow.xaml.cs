using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NebulaControls.Controls;
using NebulaControls.Demo.Views;
using NebulaControls.Theming;

namespace NebulaControls.Demo;

public partial class MainWindow : NebulaWindow
{
    private bool isSidebarCollapsed;
    private readonly DispatcherTimer toastTimer;
    private readonly ButtonsFeedbackView buttonsFeedbackView = new();
    private readonly InputsProgressView inputsProgressView = new();
    private readonly CollectionsDataView collectionsDataView = new();
    private readonly PickerLabView pickerLabView = new();
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
        toastTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        toastTimer.Tick += ToastTimer_Tick;
        buttonsFeedbackView.ToastRequested += ButtonsFeedbackView_ToastRequested;
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

    private void PickerLabNavButton_Click(object sender, RoutedEventArgs e)
    {
        ShowDemoView(pickerLabView, PickerLabNavButton);
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
            PickerLabNavButton,
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
        UpdateSidebarButton(PickerLabNavButton, "Picker Lab", "\uE787");
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

    private void ButtonsFeedbackView_ToastRequested(object? sender, ToastRequestedEventArgs e)
    {
        ShowGlobalToast(e.StyleKey, e.Title, e.Message);
    }

    private void GlobalToast_CloseClicked(object sender, EventArgs e)
    {
        HideGlobalToast();
    }

    private void ToastTimer_Tick(object? sender, EventArgs e)
    {
        HideGlobalToast();
    }

    private void ShowGlobalToast(string styleKey, string title, string message)
    {
        toastTimer.Stop();

        GlobalToast.Style = (Style)FindResource(styleKey);
        GlobalToast.Title = title;
        GlobalToast.Message = message;
        GlobalToast.Visibility = Visibility.Visible;

        AnimateGlobalToast(0, 1, null);
        toastTimer.Start();
    }

    private void HideGlobalToast()
    {
        toastTimer.Stop();
        AnimateGlobalToast(34, 0, (_, _) => GlobalToast.Visibility = Visibility.Collapsed);
    }

    private void AnimateGlobalToast(double offsetY, double opacity, EventHandler? completed)
    {
        if (GlobalToast.RenderTransform is not TranslateTransform transform)
        {
            return;
        }

        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
        var duration = TimeSpan.FromMilliseconds(220);

        var yAnimation = new DoubleAnimation
        {
            To = offsetY,
            Duration = duration,
            EasingFunction = easing
        };

        var opacityAnimation = new DoubleAnimation
        {
            To = opacity,
            Duration = duration,
            EasingFunction = easing
        };

        if (completed is not null)
        {
            opacityAnimation.Completed += completed;
        }

        transform.BeginAnimation(TranslateTransform.YProperty, yAnimation);
        GlobalToast.BeginAnimation(OpacityProperty, opacityAnimation);
    }
}

public sealed record ComponentRow(string Component, string Status, string Category);
