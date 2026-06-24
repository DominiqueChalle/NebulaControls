// Nom: NebulaDateTimePicker
// Version: V1.02
// Description: DateTime picker control exposing selected date-time and popup composition behavior.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NebulaControls.Controls;

public class NebulaDateTimePicker : Control
{
    private readonly NebulaRelayCommand selectHourCommand;
    private readonly NebulaRelayCommand selectMinuteCommand;
    private TextBox? textBox;
    private NebulaCalendarView? calendarView;
    private Popup? popup;
    private FrameworkElement? popupContent;
    private ScrollViewer? scrollHost;

    public event EventHandler? SelectedDateTimeChanged;

    static NebulaDateTimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaDateTimePicker),
            new FrameworkPropertyMetadata(typeof(NebulaDateTimePicker)));
    }

    public NebulaDateTimePicker()
    {
        selectHourCommand = new NebulaRelayCommand(SelectHour);
        selectMinuteCommand = new NebulaRelayCommand(SelectMinute);
        HourItems = new ObservableCollection<NebulaTimePickerItem>();
        MinuteItems = new ObservableCollection<NebulaTimePickerItem>();
        RefreshTimeItems();
    }

    public static readonly DependencyProperty SelectedDateTimeProperty =
        DependencyProperty.Register(
            nameof(SelectedDateTime),
            typeof(DateTime?),
            typeof(NebulaDateTimePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateTimeChanged));

    public DateTime? SelectedDateTime
    {
        get => (DateTime?)GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    public static readonly DependencyProperty DisplayDateStartProperty =
        DependencyProperty.Register(
            nameof(DisplayDateStart),
            typeof(DateTime?),
            typeof(NebulaDateTimePicker),
            new PropertyMetadata(null, OnDisplayRangeChanged));

    public DateTime? DisplayDateStart
    {
        get => (DateTime?)GetValue(DisplayDateStartProperty);
        set => SetValue(DisplayDateStartProperty, value);
    }

    public static readonly DependencyProperty DisplayDateEndProperty =
        DependencyProperty.Register(
            nameof(DisplayDateEnd),
            typeof(DateTime?),
            typeof(NebulaDateTimePicker),
            new PropertyMetadata(null, OnDisplayRangeChanged));

    public DateTime? DisplayDateEnd
    {
        get => (DateTime?)GetValue(DisplayDateEndProperty);
        set => SetValue(DisplayDateEndProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(NebulaDateTimePicker),
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
            typeof(NebulaDateTimePicker),
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
        calendarView = GetTemplateChild("PART_Calendar") as NebulaCalendarView;
        popup = GetTemplateChild("PART_Popup") as Popup;

        AttachTemplateParts();
        UpdateVisualState();
    }

    private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaDateTimePicker picker)
        {
            picker.UpdateVisualState();
            picker.SelectedDateTimeChanged?.Invoke(picker, EventArgs.Empty);
        }
    }

    private static void OnDisplayRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaDateTimePicker picker)
        {
            picker.UpdateCalendarState();
        }
    }

    private static void OnMinuteStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaDateTimePicker picker)
        {
            picker.RefreshTimeItems();
        }
    }

    private void AttachTemplateParts()
    {
        if (textBox is not null)
        {
            textBox.LostFocus += TextBox_LostFocus;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
        }

        if (calendarView is not null)
        {
            calendarView.SelectedDateCommitted += CalendarView_SelectedDateCommitted;
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

        if (calendarView is not null)
        {
            calendarView.SelectedDateCommitted -= CalendarView_SelectedDateCommitted;
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

    private void PopupContent_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (FindAncestorScrollViewer(e.OriginalSource as DependencyObject) is not null)
        {
            return;
        }

        e.Handled = true;
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

    private void CalendarView_SelectedDateCommitted(object? sender, EventArgs e)
    {
        if (calendarView?.SelectedDate is not DateTime selectedDate)
        {
            return;
        }

        var time = SelectedDateTime?.TimeOfDay ?? TimeSpan.Zero;
        SelectedDateTime = selectedDate.Date + time;
        IsDropDownOpen = true;
    }

    private void SelectHour(object? parameter)
    {
        if (parameter is not NebulaTimePickerItem item)
        {
            return;
        }

        var value = SelectedDateTime ?? DateTime.Today;
        SelectedDateTime = value.Date + new TimeSpan(item.Value, value.Minute, 0);
        IsDropDownOpen = true;
    }

    private void SelectMinute(object? parameter)
    {
        if (parameter is not NebulaTimePickerItem item)
        {
            return;
        }

        var value = SelectedDateTime ?? DateTime.Today;
        SelectedDateTime = value.Date + new TimeSpan(value.Hour, item.Value, 0);
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

        var text = textBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            SelectedDateTime = null;
            return;
        }

        var formats = new[]
        {
            "g",
            "G",
            "dd/MM/yyyy HH:mm",
            "d/M/yyyy H:mm",
            "dd-MM-yyyy HH:mm",
            "d-M-yyyy H:mm"
        };

        if ((DateTime.TryParseExact(text, formats, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedDateTime)
                || DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedDateTime))
            && IsInRange(parsedDateTime))
        {
            SelectedDateTime = new DateTime(
                parsedDateTime.Year,
                parsedDateTime.Month,
                parsedDateTime.Day,
                parsedDateTime.Hour,
                parsedDateTime.Minute,
                0);
            return;
        }

        UpdateText();
    }

    private void UpdateVisualState()
    {
        UpdateText();
        UpdateCalendarState();
        RefreshTimeItems();
    }

    private void UpdateText()
    {
        if (textBox is not null)
        {
            textBox.Text = SelectedDateTime?.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture) ?? string.Empty;
        }
    }

    private void UpdateCalendarState()
    {
        if (calendarView is null)
        {
            return;
        }

        calendarView.DisplayDateStart = DisplayDateStart;
        calendarView.DisplayDateEnd = DisplayDateEnd;
        calendarView.SelectedDate = SelectedDateTime?.Date;
        calendarView.DisplayDate = SelectedDateTime?.Date ?? DateTime.Today;
    }

    private void RefreshTimeItems()
    {
        HourItems.Clear();
        MinuteItems.Clear();

        for (var hour = 0; hour < 24; hour++)
        {
            HourItems.Add(new NebulaTimePickerItem(
                hour.ToString("00", CultureInfo.CurrentCulture),
                hour,
                SelectedDateTime?.Hour == hour));
        }

        var step = MinuteStep is < 1 or > 30 ? 5 : MinuteStep;

        for (var minute = 0; minute < 60; minute += step)
        {
            MinuteItems.Add(new NebulaTimePickerItem(
                minute.ToString("00", CultureInfo.CurrentCulture),
                minute,
                SelectedDateTime?.Minute == minute));
        }
    }

    private bool IsInRange(DateTime value)
    {
        return (DisplayDateStart is null || value.Date >= DisplayDateStart.Value.Date)
            && (DisplayDateEnd is null || value.Date <= DisplayDateEnd.Value.Date);
    }
}
