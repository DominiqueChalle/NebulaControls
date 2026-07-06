// Nom: NebulaWindow
// Version: V1.01
// Description: Custom Nebula application window shell with title bar, window commands and themed content surface.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using System.Runtime.InteropServices;

namespace NebulaControls.Controls;

public class NebulaWindow : Window
{
    private const int DwmwaWindowCornerPreference = 33;
    private const string MaximizeGlyph = "\uE922";
    private const string RestoreGlyph = "\uE923";

    private enum DwmWindowCornerPreference
    {
        Default = 0,
        DoNotRound = 1,
        Round = 2,
        RoundSmall = 3
    }

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(NebulaWindow),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconContentProperty =
        DependencyProperty.Register(
            nameof(IconContent),
            typeof(object),
            typeof(NebulaWindow),
            new PropertyMetadata("N"));

    public static readonly DependencyProperty TitleBarContentProperty =
        DependencyProperty.Register(
            nameof(TitleBarContent),
            typeof(object),
            typeof(NebulaWindow),
            new PropertyMetadata(null));

    private Rect restoreBoundsBeforeMaximize;
    private Border? windowFrame;
    private FrameworkElement? titleBar;
    private Button? minimizeButton;
    private Button? maximizeButton;
    private Button? closeButton;
    private bool isCustomMaximized;

    static NebulaWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaWindow),
            new FrameworkPropertyMetadata(typeof(NebulaWindow)));
    }

    public NebulaWindow()
    {
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.CanResizeWithGrip;
        SetResourceReference(BackgroundProperty, "Brush.Background");

        WindowChrome.SetWindowChrome(
            this,
            new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                ResizeBorderThickness = new Thickness(8),
                UseAeroCaptionButtons = false
            });
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public object? IconContent
    {
        get => GetValue(IconContentProperty);
        set => SetValue(IconContentProperty, value);
    }

    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DetachTemplateEvents();

        windowFrame = GetTemplateChild("PART_WindowFrame") as Border;
        titleBar = GetTemplateChild("PART_TitleBar") as FrameworkElement;
        minimizeButton = GetTemplateChild("PART_MinimizeButton") as Button;
        maximizeButton = GetTemplateChild("PART_MaximizeButton") as Button;
        closeButton = GetTemplateChild("PART_CloseButton") as Button;

        AttachTemplateEvents();
        UpdateFrameShape();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);

        if (WindowState == WindowState.Normal && isCustomMaximized)
        {
            isCustomMaximized = false;
            UpdateFrameShape();
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ApplyNativeRoundedCorners();
    }

    private void AttachTemplateEvents()
    {
        if (titleBar is not null)
        {
            titleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
        }

        if (minimizeButton is not null)
        {
            minimizeButton.Click += MinimizeButton_Click;
            minimizeButton.PreviewMouseLeftButtonDown += MinimizeButton_PreviewMouseLeftButtonDown;
        }

        if (maximizeButton is not null)
        {
            maximizeButton.Click += MaximizeButton_Click;
            maximizeButton.PreviewMouseLeftButtonDown += MaximizeButton_PreviewMouseLeftButtonDown;
        }

        if (closeButton is not null)
        {
            closeButton.Click += CloseButton_Click;
            closeButton.PreviewMouseLeftButtonDown += CloseButton_PreviewMouseLeftButtonDown;
        }
    }

    private void DetachTemplateEvents()
    {
        if (titleBar is not null)
        {
            titleBar.MouseLeftButtonDown -= TitleBar_MouseLeftButtonDown;
        }

        if (minimizeButton is not null)
        {
            minimizeButton.Click -= MinimizeButton_Click;
            minimizeButton.PreviewMouseLeftButtonDown -= MinimizeButton_PreviewMouseLeftButtonDown;
        }

        if (maximizeButton is not null)
        {
            maximizeButton.Click -= MaximizeButton_Click;
            maximizeButton.PreviewMouseLeftButtonDown -= MaximizeButton_PreviewMouseLeftButtonDown;
        }

        if (closeButton is not null)
        {
            closeButton.Click -= CloseButton_Click;
            closeButton.PreviewMouseLeftButtonDown -= CloseButton_PreviewMouseLeftButtonDown;
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsInsideWindowButton(e.OriginalSource as DependencyObject))
        {
            return;
        }

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

    private static bool IsInsideWindowButton(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is Button)
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MinimizeButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    private void MaximizeButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        ToggleWindowState();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CloseButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
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
        UpdateFrameShape();
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
        UpdateFrameShape();
    }

    private void UpdateFrameShape()
    {
        if (windowFrame is null)
        {
            return;
        }

        windowFrame.CornerRadius = isCustomMaximized ? new CornerRadius(0) : new CornerRadius(10);

        if (maximizeButton is not null)
        {
            maximizeButton.Content = isCustomMaximized ? RestoreGlyph : MaximizeGlyph;
        }
    }

    private void ApplyNativeRoundedCorners()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            return;
        }

        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var preference = (int)DwmWindowCornerPreference.Round;
        _ = DwmSetWindowAttribute(
            handle,
            DwmwaWindowCornerPreference,
            ref preference,
            sizeof(int));
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        ref int pvAttribute,
        int cbAttribute);
}
