using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaDatePicker : Control
{
    private TextBox? textBox;
    private Popup? popup;
    private NebulaCalendarView? calendarView;

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
            new PropertyMetadata(false));

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

        if (textBox is not null)
        {
            textBox.LostFocus += TextBox_LostFocus;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
        }

        if (calendarView is not null)
        {
            calendarView.SelectedDateCommitted += CalendarView_SelectedDateCommitted;
        }

        UpdateText();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        e.Handled = IsDropDownOpen;
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

    private void CalendarView_SelectedDateCommitted(object? sender, EventArgs e)
    {
        IsDropDownOpen = false;
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
    }
}
