// Nom: NebulaTimePicker
// Version: V1.03
// Description: Time picker control exposing selected time, manual input and clock popup behavior.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NebulaControls.Controls;

public class NebulaTimePicker : Control
{
    private readonly NebulaRelayCommand selectHourCommand;
    private readonly NebulaRelayCommand selectMinuteCommand;
    private readonly NebulaRelayCommand selectClockItemCommand;
    private readonly NebulaRelayCommand showHourModeCommand;
    private readonly NebulaRelayCommand showMinuteModeCommand;
    private NebulaTimePickerClockMode clockMode = NebulaTimePickerClockMode.Hour;
    private TextBox? textBox;
    private Popup? popup;
    private FrameworkElement? clockFace;
    private FrameworkElement? popupContent;
    private ScrollViewer? scrollHost;
    private Window? keyboardWindow;
    private TimeSpan? selectedTimeBeforeOpen;
    private bool isDraggingClock;

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
        selectClockItemCommand = new NebulaRelayCommand(SelectClockItem);
        showHourModeCommand = new NebulaRelayCommand(_ => SetClockMode(NebulaTimePickerClockMode.Hour));
        showMinuteModeCommand = new NebulaRelayCommand(_ => SetClockMode(NebulaTimePickerClockMode.Minute));
        HourItems = new ObservableCollection<NebulaTimePickerItem>();
        MinuteItems = new ObservableCollection<NebulaTimePickerItem>();
        ClockItems = new ObservableCollection<NebulaTimeClockItem>();
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

    public ObservableCollection<NebulaTimeClockItem> ClockItems { get; }

    public ICommand SelectHourCommand => selectHourCommand;

    public ICommand SelectMinuteCommand => selectMinuteCommand;

    public ICommand SelectClockItemCommand => selectClockItemCommand;

    public ICommand ShowHourModeCommand => showHourModeCommand;

    public ICommand ShowMinuteModeCommand => showMinuteModeCommand;

    public string ClockModeTitle => clockMode == NebulaTimePickerClockMode.Hour
        ? "Heures"
        : "Minutes";

    private static readonly DependencyPropertyKey ClockHandXPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ClockHandX),
            typeof(double),
            typeof(NebulaTimePicker),
            new PropertyMetadata(110d));

    public static readonly DependencyProperty ClockHandXProperty = ClockHandXPropertyKey.DependencyProperty;

    public double ClockHandX
    {
        get => (double)GetValue(ClockHandXProperty);
        private set => SetValue(ClockHandXPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ClockHandYPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ClockHandY),
            typeof(double),
            typeof(NebulaTimePicker),
            new PropertyMetadata(22d));

    public static readonly DependencyProperty ClockHandYProperty = ClockHandYPropertyKey.DependencyProperty;

    public double ClockHandY
    {
        get => (double)GetValue(ClockHandYProperty);
        private set => SetValue(ClockHandYPropertyKey, value);
    }

    public override void OnApplyTemplate()
    {
        DetachTemplateParts();

        base.OnApplyTemplate();

        textBox = GetTemplateChild("PART_TextBox") as TextBox;
        popup = GetTemplateChild("PART_Popup") as Popup;
        clockFace = GetTemplateChild("PART_ClockFace") as FrameworkElement;

        AttachTemplateParts();
        UpdateText();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        HandleTimePickerKeyDown(e);
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

        if (clockFace is not null)
        {
            clockFace.PreviewMouseLeftButtonDown += ClockFace_PreviewMouseLeftButtonDown;
            clockFace.PreviewMouseMove += ClockFace_PreviewMouseMove;
            clockFace.PreviewMouseLeftButtonUp += ClockFace_PreviewMouseLeftButtonUp;
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

        if (clockFace is not null)
        {
            clockFace.PreviewMouseLeftButtonDown -= ClockFace_PreviewMouseLeftButtonDown;
            clockFace.PreviewMouseMove -= ClockFace_PreviewMouseMove;
            clockFace.PreviewMouseLeftButtonUp -= ClockFace_PreviewMouseLeftButtonUp;
        }

        DetachPopupContent();
        DetachScrollHost();
    }

    private void Popup_Opened(object? sender, EventArgs e)
    {
        selectedTimeBeforeOpen = SelectedTime;
        SetClockMode(NebulaTimePickerClockMode.Hour);

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
            popupContent.Focusable = true;
            popupContent.Dispatcher.BeginInvoke(() => Keyboard.Focus(popupContent), DispatcherPriority.Input);
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
        HandleTimePickerKeyDown(e);
    }

    private void KeyboardWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (IsDropDownOpen)
        {
            HandleTimePickerKeyDown(e);
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

    private void DetachPopupContent()
    {
        if (popupContent is not null)
        {
            popupContent.PreviewMouseWheel -= PopupContent_PreviewMouseWheel;
            popupContent.PreviewKeyDown -= PopupContent_PreviewKeyDown;
            popupContent = null;
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

    private void SetClockMode(NebulaTimePickerClockMode mode)
    {
        clockMode = mode;
        RefreshClockItems();
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
        SetClockMode(NebulaTimePickerClockMode.Minute);
    }

    private void SelectMinute(object? parameter)
    {
        if (parameter is not NebulaTimePickerItem item)
        {
            return;
        }

        var hour = SelectedTime?.Hours ?? 0;
        SelectedTime = new TimeSpan(hour, item.Value, 0);
    }

    private void SelectClockItem(object? parameter)
    {
        if (parameter is not NebulaTimeClockItem item)
        {
            return;
        }

        if (clockMode == NebulaTimePickerClockMode.Hour)
        {
            var minute = SelectedTime?.Minutes ?? 0;
            SelectedTime = new TimeSpan(item.Value, minute, 0);
            SetClockMode(NebulaTimePickerClockMode.Minute);
            return;
        }

        var hour = SelectedTime?.Hours ?? 0;
        SelectedTime = new TimeSpan(hour, item.Value, 0);
    }

    private void ClockFace_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (clockFace is null)
        {
            return;
        }

        isDraggingClock = true;
        clockFace.CaptureMouse();
        SelectClockValueFromPoint(e.GetPosition(clockFace));
        e.Handled = true;
    }

    private void ClockFace_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!isDraggingClock || clockFace is null)
        {
            return;
        }

        SelectClockValueFromPoint(e.GetPosition(clockFace));
        e.Handled = true;
    }

    private void ClockFace_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!isDraggingClock)
        {
            return;
        }

        isDraggingClock = false;
        clockFace?.ReleaseMouseCapture();

        if (clockMode == NebulaTimePickerClockMode.Hour)
        {
            SetClockMode(NebulaTimePickerClockMode.Minute);
        }

        e.Handled = true;
    }

    private void SelectClockValueFromPoint(Point point)
    {
        const double center = 110;
        var deltaX = point.X - center;
        var deltaY = point.Y - center;
        var angle = Math.Atan2(deltaY, deltaX) * 180 / Math.PI + 90;

        if (angle < 0)
        {
            angle += 360;
        }

        if (clockMode == NebulaTimePickerClockMode.Hour)
        {
            SelectClockHourFromAngle(angle, Math.Sqrt(deltaX * deltaX + deltaY * deltaY));
            return;
        }

        SelectClockMinuteFromAngle(angle);
    }

    private void SelectClockHourFromAngle(double angle, double distance)
    {
        var position = (int)Math.Round(angle / 30) % 12;
        var isInner = distance < 73;
        var hour = isInner
            ? position == 0 ? 0 : position + 12
            : position == 0 ? 12 : position;
        var minute = SelectedTime?.Minutes ?? 0;

        SelectedTime = new TimeSpan(hour, minute, 0);
    }

    private void SelectClockMinuteFromAngle(double angle)
    {
        var step = MinuteStep is < 1 or > 30 ? 5 : MinuteStep;
        var minute = (int)Math.Round(angle / 6) % 60;
        minute = (int)(Math.Round(minute / (double)step) * step) % 60;
        var hour = SelectedTime?.Hours ?? 0;

        SelectedTime = new TimeSpan(hour, minute, 0);
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        CommitText();
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        HandleTimePickerKeyDown(e);
    }

    private void HandleTimePickerKeyDown(KeyEventArgs e)
    {
        if (e.Handled || !IsEnabled)
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            CommitText();
            IsDropDownOpen = false;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            if (IsDropDownOpen)
            {
                RestoreTimeBeforeOpen();
                IsDropDownOpen = false;
            }
            else
            {
                UpdateText();
            }

            e.Handled = true;
            return;
        }

        if (e.Key == Key.F4 || (e.Key == Key.Down && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt))
        {
            IsDropDownOpen = !IsDropDownOpen;
            e.Handled = true;
        }
    }

    private void RestoreTimeBeforeOpen()
    {
        SelectedTime = selectedTimeBeforeOpen;
        UpdateText();
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

        RefreshClockItems();
    }

    private void RefreshClockItems()
    {
        ClockItems.Clear();

        if (clockMode == NebulaTimePickerClockMode.Hour)
        {
            AddHourClockItems();
            NotifyClockHandChanged();
            return;
        }

        AddMinuteClockItems();
        NotifyClockHandChanged();
    }

    private void AddHourClockItems()
    {
        for (var hour = 1; hour <= 12; hour++)
        {
            AddClockItem(
                hour.ToString("00", CultureInfo.CurrentCulture),
                hour,
                hour,
                isInner: false,
                SelectedTime?.Hours == hour);
        }

        for (var hour = 13; hour <= 23; hour++)
        {
            AddClockItem(
                hour.ToString("00", CultureInfo.CurrentCulture),
                hour,
                hour - 12,
                isInner: true,
                SelectedTime?.Hours == hour);
        }

        AddClockItem("00", 0, 12, isInner: true, SelectedTime?.Hours == 0);
    }

    private void AddMinuteClockItems()
    {
        var step = MinuteStep is < 1 or > 30 ? 5 : MinuteStep;

        for (var minute = 0; minute < 60; minute += step)
        {
            var clockPosition = minute == 0 ? 12 : minute / 5;
            AddClockItem(
                minute.ToString("00", CultureInfo.CurrentCulture),
                minute,
                clockPosition,
                isInner: false,
                SelectedTime?.Minutes == minute);
        }
    }

    private void AddClockItem(string label, int value, int clockPosition, bool isInner, bool isSelected)
    {
        const double center = 110;
        var radius = isInner ? 58 : 88;
        var angle = (clockPosition * 30 - 90) * Math.PI / 180;
        var x = center + radius * Math.Cos(angle) - 17;
        var y = center + radius * Math.Sin(angle) - 17;

        ClockItems.Add(new NebulaTimeClockItem(label, value, x, y, x + 17, y + 17, isSelected, isInner));
    }

    private void NotifyClockHandChanged()
    {
        var selectedItem = GetSelectedClockItem();

        ClockHandX = selectedItem?.CenterX ?? 110;
        ClockHandY = selectedItem?.CenterY ?? 22;
    }

    private NebulaTimeClockItem? GetSelectedClockItem()
    {
        foreach (var item in ClockItems)
        {
            if (item.IsSelected)
            {
                return item;
            }
        }

        return null;
    }

    private enum NebulaTimePickerClockMode
    {
        Hour,
        Minute
    }
}
