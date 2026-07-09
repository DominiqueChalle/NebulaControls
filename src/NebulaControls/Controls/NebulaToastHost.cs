// Nom: NebulaToastHost
// Version: V1.01
// Description: Toast host control managing stacked notifications, auto-dismiss timers and close commands.

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NebulaControls.Controls;

public class NebulaToastHost : ItemsControl
{
    private readonly NebulaRelayCommand closeToastCommand;

    static NebulaToastHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaToastHost),
            new FrameworkPropertyMetadata(typeof(NebulaToastHost)));
    }

    public NebulaToastHost()
    {
        closeToastCommand = new NebulaRelayCommand(CloseToast);
        ItemsSource = Toasts;
    }

    public ObservableCollection<NebulaToastNotification> Toasts { get; } = [];

    public ICommand CloseToastCommand => closeToastCommand;

    public static readonly DependencyProperty ToastDurationProperty =
        DependencyProperty.Register(
            nameof(ToastDuration),
            typeof(TimeSpan),
            typeof(NebulaToastHost),
            new PropertyMetadata(TimeSpan.FromSeconds(5)));

    public TimeSpan ToastDuration
    {
        get => (TimeSpan)GetValue(ToastDurationProperty);
        set => SetValue(ToastDurationProperty, value);
    }

    public static readonly DependencyProperty MaxToastsProperty =
        DependencyProperty.Register(
            nameof(MaxToasts),
            typeof(int),
            typeof(NebulaToastHost),
            new PropertyMetadata(4, OnMaxToastsChanged));

    public int MaxToasts
    {
        get => (int)GetValue(MaxToastsProperty);
        set => SetValue(MaxToastsProperty, value);
    }

    public void Show(string styleKey, string title, string message)
    {
        Show((Style)FindResource(styleKey), title, message);
    }

    public void Show(NebulaToastVariant variant, string title, string message)
    {
        Show(GetStyleKey(variant), title, message);
    }

    public void ShowInfo(string title, string message)
    {
        Show(NebulaToastVariant.Info, title, message);
    }

    public void ShowSuccess(string title, string message)
    {
        Show(NebulaToastVariant.Success, title, message);
    }

    public void ShowWarning(string title, string message)
    {
        Show(NebulaToastVariant.Warning, title, message);
    }

    public void ShowDanger(string title, string message)
    {
        Show(NebulaToastVariant.Danger, title, message);
    }

    public void Show(Style style, string title, string message)
    {
        var toast = new NebulaToastNotification(style, title, message);
        Toasts.Add(toast);
        TrimToMaxToasts();

        if (ToastDuration <= TimeSpan.Zero)
        {
            return;
        }

        var timer = new DispatcherTimer
        {
            Interval = ToastDuration
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            Toasts.Remove(toast);
        };
        timer.Start();
    }

    public void Clear()
    {
        Toasts.Clear();
    }

    private static void OnMaxToastsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaToastHost host)
        {
            host.TrimToMaxToasts();
        }
    }

    private void TrimToMaxToasts()
    {
        var maxToasts = Math.Max(1, MaxToasts);
        while (Toasts.Count > maxToasts)
        {
            Toasts.RemoveAt(0);
        }
    }

    private void CloseToast(object? parameter)
    {
        if (parameter is NebulaToastNotification toast)
        {
            Toasts.Remove(toast);
        }
    }

    private static string GetStyleKey(NebulaToastVariant variant)
    {
        return variant switch
        {
            NebulaToastVariant.Success => "NebulaSuccessToast",
            NebulaToastVariant.Warning => "NebulaWarningToast",
            NebulaToastVariant.Danger => "NebulaDangerToast",
            _ => "NebulaInfoToast"
        };
    }
}

public sealed record NebulaToastNotification(Style Style, string Title, string Message);
