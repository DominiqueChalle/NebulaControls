// Nom: NebulaDatePicker
// Version: V1.03
// Description: Date picker control exposing selected date, display text, date range and popup behavior.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NebulaControls.Controls;

public class NebulaDatePicker : Control
{
    private TextBox? textBox;
    private Popup? popup;
    private FrameworkElement? popupContent;
    private ScrollViewer? scrollHost;
    private Window? keyboardWindow;
    private NebulaCalendarView? calendarView;
    private DateTime? selectedDateBeforeOpen;

    public event EventHandler? SelectedDateChanged;

    static NebulaDatePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaDatePicker),
            new FrameworkPropertyMetadata(typeof(NebulaDatePicker)));
    }

    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(NebulaDatePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public static readonly DependencyProperty DisplayDateProperty =
        DependencyProperty.Register(
            nameof(DisplayDate),
            typeof(DateTime),
            typeof(NebulaDatePicker),
            new PropertyMetadata(DateTime.Today));

    public DateTime DisplayDate
    {
        get => (DateTime)GetValue(DisplayDateProperty);
        set => SetValue(DisplayDateProperty, value);
    }

    public static readonly DependencyProperty DisplayDateStartProperty =
        DependencyProperty.Register(
            nameof(DisplayDateStart),
            typeof(DateTime?),
            typeof(NebulaDatePicker),
            new PropertyMetadata(null));

    public DateTime? DisplayDateStart
    {
        get => (DateTime?)GetValue(DisplayDateStartProperty);
        set => SetValue(DisplayDateStartProperty, value);
    }

    public static readonly DependencyProperty DisplayDateEndProperty =
        DependencyProperty.Register(
            nameof(DisplayDateEnd),
            typeof(DateTime?),
            typeof(NebulaDatePicker),
            new PropertyMetadata(null));

    public DateTime? DisplayDateEnd
    {
        get => (DateTime?)GetValue(DisplayDateEndProperty);
        set => SetValue(DisplayDateEndProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(NebulaDatePicker),
            new PropertyMetadata(false, OnIsDropDownOpenChanged));

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(NebulaDatePicker),
            new PropertyMetadata("Select date"));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public override void OnApplyTemplate()
    {
        DetachTemplateParts();
        base.OnApplyTemplate();

        textBox = GetTemplateChild("PART_TextBox") as TextBox;
        popup = GetTemplateChild("PART_Popup") as Popup;
        calendarView = GetTemplateChild("PART_Calendar") as NebulaCalendarView;

        AttachTemplateParts();

        UpdateText();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        e.Handled = IsDropDownOpen;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        HandleDatePickerKeyDown(e);
    }

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaDatePicker datePicker)
        {
            if (e.NewValue is DateTime selectedDate)
            {
                datePicker.DisplayDate = selectedDate;
            }

            datePicker.UpdateText();
            datePicker.SelectedDateChanged?.Invoke(datePicker, EventArgs.Empty);
        }
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaDatePicker { IsDropDownOpen: true } datePicker)
        {
            datePicker.PrepareCalendarForOpen();
        }
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        CommitText();
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        HandleDatePickerKeyDown(e);
    }

    private void HandleDatePickerKeyDown(KeyEventArgs e)
    {
        if (e.Handled || !IsEnabled)
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            RestoreDateBeforeOpen();
            IsDropDownOpen = false;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter)
        {
            if (!IsDropDownOpen)
            {
                CommitText();
                IsDropDownOpen = true;
                e.Handled = true;
                return;
            }

            CommitText();
            IsDropDownOpen = false;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.F4 || (e.Key == Key.Down && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt))
        {
            IsDropDownOpen = !IsDropDownOpen;
            e.Handled = true;
            return;
        }

        if (!IsDropDownOpen)
        {
            return;
        }

        if (e.Key == Key.Tab)
        {
            e.Handled = true;
            return;
        }

        switch (e.Key)
        {
            case Key.Left:
                MoveKeyboardDateByDays(-1);
                e.Handled = true;
                break;
            case Key.Right:
                MoveKeyboardDateByDays(1);
                e.Handled = true;
                break;
            case Key.Up:
                MoveKeyboardDateByDays(-7);
                e.Handled = true;
                break;
            case Key.Down:
                MoveKeyboardDateByDays(7);
                e.Handled = true;
                break;
            case Key.PageUp:
                MoveKeyboardDateByMonths(-1);
                e.Handled = true;
                break;
            case Key.PageDown:
                MoveKeyboardDateByMonths(1);
                e.Handled = true;
                break;
            case Key.Home:
                MoveKeyboardDateToMonthBoundary(startOfMonth: true);
                e.Handled = true;
                break;
            case Key.End:
                MoveKeyboardDateToMonthBoundary(startOfMonth: false);
                e.Handled = true;
                break;
        }
    }

    private void CalendarView_SelectedDateCommitted(object? sender, EventArgs e)
    {
        IsDropDownOpen = false;
        UpdateText();
    }

    private void PrepareCalendarForOpen()
    {
        selectedDateBeforeOpen = SelectedDate?.Date;
        var targetDate = SelectedDate?.Date ?? GetDefaultDisplayDate();

        DisplayDate = targetDate;

        if (calendarView is not null)
        {
            calendarView.SetCurrentValue(NebulaCalendarView.DisplayModeProperty, NebulaCalendarMode.Month);
            calendarView.SetCurrentValue(NebulaCalendarView.DisplayDateProperty, targetDate);
        }
    }

    private DateTime GetDefaultDisplayDate()
    {
        if (IsInRange(DateTime.Today))
        {
            return DateTime.Today;
        }

        if (DisplayDateStart is DateTime start)
        {
            return start.Date;
        }

        if (DisplayDateEnd is DateTime end)
        {
            return end.Date;
        }

        return DisplayDate.Date;
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
            popupContent.PreviewKeyDown += PopupContent_PreviewKeyDown;
        }

        keyboardWindow = Window.GetWindow(this);

        if (keyboardWindow is not null)
        {
            keyboardWindow.PreviewKeyDown += KeyboardWindow_PreviewKeyDown;
        }
    }

    private void Popup_Closed(object? sender, EventArgs e)
    {
        DetachPopupContent();
        DetachScrollHost();
        DetachKeyboardWindow();
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

    private void PopupContent_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        HandleDatePickerKeyDown(e);
    }

    private void KeyboardWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (IsDropDownOpen)
        {
            HandleDatePickerKeyDown(e);
        }
    }

    private void DetachScrollHost()
    {
        if (scrollHost is not null)
        {
            scrollHost.PreviewMouseWheel -= ScrollHost_PreviewMouseWheel;
            scrollHost = null;
        }
    }

    private void DetachKeyboardWindow()
    {
        if (keyboardWindow is not null)
        {
            keyboardWindow.PreviewKeyDown -= KeyboardWindow_PreviewKeyDown;
            keyboardWindow = null;
        }
    }

    private void DetachPopupContent()
    {
        if (popupContent is not null)
        {
            popupContent.PreviewMouseWheel -= PopupContent_PreviewMouseWheel;
            popupContent.PreviewKeyDown -= PopupContent_PreviewKeyDown;
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

    private void CommitText()
    {
        if (textBox is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(textBox.Text))
        {
            SelectedDate = null;
            return;
        }

        if (DateTime.TryParse(textBox.Text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedDate)
            && IsInRange(parsedDate))
        {
            SelectedDate = parsedDate.Date;
            return;
        }

        UpdateText();
    }

    private bool IsInRange(DateTime date)
    {
        return (DisplayDateStart is null || date.Date >= DisplayDateStart.Value.Date)
            && (DisplayDateEnd is null || date.Date <= DisplayDateEnd.Value.Date);
    }

    private void MoveKeyboardDateByDays(int days)
    {
        MoveKeyboardDate(GetKeyboardBaseDate().AddDays(days));
    }

    private void MoveKeyboardDateByMonths(int months)
    {
        MoveKeyboardDate(GetKeyboardBaseDate().AddMonths(months));
    }

    private void MoveKeyboardDateToMonthBoundary(bool startOfMonth)
    {
        var baseDate = GetKeyboardBaseDate();
        var targetDate = startOfMonth
            ? new DateTime(baseDate.Year, baseDate.Month, 1)
            : new DateTime(baseDate.Year, baseDate.Month, DateTime.DaysInMonth(baseDate.Year, baseDate.Month));

        MoveKeyboardDate(targetDate);
    }

    private DateTime GetKeyboardBaseDate()
    {
        if (SelectedDate is DateTime selectedDate)
        {
            return selectedDate.Date;
        }

        return IsInRange(DateTime.Today)
            ? DateTime.Today
            : DisplayDate.Date;
    }

    private void MoveKeyboardDate(DateTime date)
    {
        date = ClampToRange(date);

        SelectedDate = date.Date;
        DisplayDate = date.Date;

        if (calendarView is not null)
        {
            calendarView.SetCurrentValue(NebulaCalendarView.DisplayDateProperty, date.Date);
        }
    }

    private DateTime ClampToRange(DateTime date)
    {
        if (DisplayDateStart is DateTime start && date.Date < start.Date)
        {
            return start.Date;
        }

        if (DisplayDateEnd is DateTime end && date.Date > end.Date)
        {
            return end.Date;
        }

        return date.Date;
    }

    private void RestoreDateBeforeOpen()
    {
        SelectedDate = selectedDateBeforeOpen;

        if (selectedDateBeforeOpen is DateTime restoredDate)
        {
            DisplayDate = restoredDate.Date;
        }

        UpdateText();
    }

    private void UpdateText()
    {
        if (textBox is not null)
        {
            textBox.Text = SelectedDate?.ToString("d", CultureInfo.CurrentCulture) ?? string.Empty;
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
        DetachKeyboardWindow();
    }
}
