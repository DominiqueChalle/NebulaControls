using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NebulaControls.Controls;

public class NebulaTimePicker : Control
{
    private readonly NebulaRelayCommand selectHourCommand;
    private readonly NebulaRelayCommand selectMinuteCommand;
    private TextBox? textBox;
    private Popup? popup;
    private FrameworkElement? popupContent;
    private ScrollViewer? scrollHost;

    public event EventHandler? SelectedTimeChanged;

    static NebulaTimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaTimePicker),
            new FrameworkPropertyMetadata(typeof(NebulaTimePicker)));
    }

    public NebulaTimePicker()
    {
        selectHourCommand = new NebulaRelayCommand(SelectHour);
        selectMinuteCommand = new NebulaRelayCommand(SelectMinute);
        HourItems = new ObservableCollection<NebulaTimePickerItem>();
        MinuteItems = new ObservableCollection<NebulaTimePickerItem>();
        RefreshItems();
    }

    public static readonly DependencyProperty SelectedTimeProperty =
        DependencyProperty.Register(
            nameof(SelectedTime),
            typeof(TimeSpan?),
            typeof(NebulaTimePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTimeChanged));

    public TimeSpan? SelectedTime
    {
        get => (TimeSpan?)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(NebulaTimePicker),
            new PropertyMetadata(false));

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly DependencyProperty MinuteStepProperty =
        DependencyProperty.Register(
            nameof(MinuteStep),
            typeof(int),
            typeof(NebulaTimePicker),
            new PropertyMetadata(5, OnMinuteStepChanged));

    public int MinuteStep
    {
        get => (int)GetValue(MinuteStepProperty);
        set => SetValue(MinuteStepProperty, value);
    }

    public ObservableCollection<NebulaTimePickerItem> HourItems { get; }

    public ObservableCollection<NebulaTimePickerItem> MinuteItems { get; }

    public ICommand SelectHourCommand => selectHourCommand;

    public ICommand SelectMinuteCommand => selectMinuteCommand;

    public override void OnApplyTemplate()
    {
        DetachTemplateParts();

        base.OnApplyTemplate();

        textBox = GetTemplateChild("PART_TextBox") as TextBox;
        popup = GetTemplateChild("PART_Popup") as Popup;

        AttachTemplateParts();
        UpdateText();
    }

    private void AttachTemplateParts()
    {
        if (textBox is not null)
        {
            textBox.LostFocus += TextBox_LostFocus;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
        }

        if (popup is not null)
        {
            popup.Opened += Popup_Opened;
            popup.Closed += Popup_Closed;
        }
    }

    private void DetachTemplateParts()
    {
        if (textBox is not null)
        {
            textBox.LostFocus -= TextBox_LostFocus;
            textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
        }

        if (popup is not null)
        {
            popup.Opened -= Popup_Opened;
            popup.Closed -= Popup_Closed;
        }

        DetachPopupContent();
        DetachScrollHost();
    }

    private void Popup_Opened(object? sender, EventArgs e)
    {
        scrollHost = FindAncestorScrollViewer(this);

        if (scrollHost is not null)
        {
            scrollHost.PreviewMouseWheel += ScrollHost_PreviewMouseWheel;
        }

        popupContent = popup?.Child as FrameworkElement;

        if (popupContent is not null)
        {
            popupContent.PreviewMouseWheel += PopupContent_PreviewMouseWheel;
        }
    }

    private void Popup_Closed(object? sender, EventArgs e)
    {
        DetachPopupContent();
        DetachScrollHost();
    }

    private void ScrollHost_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!IsDropDownOpen)
        {
            return;
        }

        if (popup?.Child is FrameworkElement { IsMouseOver: true })
        {
            return;
        }

        e.Handled = true;
    }

    private void PopupContent_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (FindAncestorScrollViewer(e.OriginalSource as DependencyObject) is not null)
        {
            return;
        }

        e.Handled = true;
    }

    private void DetachScrollHost()
    {
        if (scrollHost is not null)
        {
            scrollHost.PreviewMouseWheel -= ScrollHost_PreviewMouseWheel;
            scrollHost = null;
        }
    }

    private void DetachPopupContent()
    {
        if (popupContent is not null)
        {
            popupContent.PreviewMouseWheel -= PopupContent_PreviewMouseWheel;
            popupContent = null;
        }
    }

    private static ScrollViewer? FindAncestorScrollViewer(DependencyObject? current)
    {
        while (current is not null)
        {
            if (current is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaTimePicker timePicker)
        {
            timePicker.UpdateText();
            timePicker.RefreshItems();
            timePicker.SelectedTimeChanged?.Invoke(timePicker, EventArgs.Empty);
        }
    }

    private static void OnMinuteStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaTimePicker timePicker)
        {
            timePicker.RefreshItems();
        }
    }

    private void SelectHour(object? parameter)
    {
        if (parameter is not NebulaTimePickerItem item)
        {
            return;
        }

        var minute = SelectedTime?.Minutes ?? 0;
        SelectedTime = new TimeSpan(item.Value, minute, 0);
        IsDropDownOpen = true;
    }

    private void SelectMinute(object? parameter)
    {
        if (parameter is not NebulaTimePickerItem item)
        {
            return;
        }

        var hour = SelectedTime?.Hours ?? 0;
        SelectedTime = new TimeSpan(hour, item.Value, 0);
        IsDropDownOpen = false;
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        CommitText();
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitText();
            IsDropDownOpen = false;
            e.Handled = true;
        }

        if (e.Key == Key.Escape)
        {
            UpdateText();
            IsDropDownOpen = false;
            e.Handled = true;
        }
    }

    private void CommitText()
    {
        if (textBox is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(textBox.Text))
        {
            SelectedTime = null;
            return;
        }

        if (TimeSpan.TryParseExact(textBox.Text.Trim(), @"h\:mm", CultureInfo.CurrentCulture, out var parsedTime)
            || TimeSpan.TryParseExact(textBox.Text.Trim(), @"hh\:mm", CultureInfo.CurrentCulture, out parsedTime))
        {
            if (parsedTime >= TimeSpan.Zero && parsedTime < TimeSpan.FromDays(1))
            {
                SelectedTime = new TimeSpan(parsedTime.Hours, parsedTime.Minutes, 0);
                return;
            }
        }

        UpdateText();
    }

    private void UpdateText()
    {
        if (textBox is not null)
        {
            textBox.Text = SelectedTime?.ToString(@"hh\:mm", CultureInfo.CurrentCulture) ?? string.Empty;
        }
    }

    private void RefreshItems()
    {
        HourItems.Clear();
        MinuteItems.Clear();

        for (var hour = 0; hour < 24; hour++)
        {
            HourItems.Add(new NebulaTimePickerItem(
                hour.ToString("00", CultureInfo.CurrentCulture),
                hour,
                SelectedTime?.Hours == hour));
        }

        var step = MinuteStep is < 1 or > 30 ? 5 : MinuteStep;

        for (var minute = 0; minute < 60; minute += step)
        {
            MinuteItems.Add(new NebulaTimePickerItem(
                minute.ToString("00", CultureInfo.CurrentCulture),
                minute,
                SelectedTime?.Minutes == minute));
        }
    }
}
