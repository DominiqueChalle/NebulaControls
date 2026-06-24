// Nom: NebulaCalendarView
// Version: V1.02
// Description: Calendar selection view used by date and datetime picker controls.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaCalendarView : Control
{
    private readonly NebulaRelayCommand selectItemCommand;

    static NebulaCalendarView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaCalendarView),
            new FrameworkPropertyMetadata(typeof(NebulaCalendarView)));
    }

    public NebulaCalendarView()
    {
        selectItemCommand = new NebulaRelayCommand(SelectItem, CanSelectItem);
        PreviousCommand = new NebulaRelayCommand(_ => MovePrevious());
        NextCommand = new NebulaRelayCommand(_ => MoveNext());
        HeaderCommand = new NebulaRelayCommand(_ => MoveUp());
        Items = new ObservableCollection<NebulaCalendarItem>();
        DisplayDate = DateTime.Today;
    }

    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(NebulaCalendarView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCalendarStateChanged));

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public static readonly DependencyProperty DisplayDateProperty =
        DependencyProperty.Register(
            nameof(DisplayDate),
            typeof(DateTime),
            typeof(NebulaCalendarView),
            new PropertyMetadata(DateTime.Today, OnCalendarStateChanged));

    public DateTime DisplayDate
    {
        get => (DateTime)GetValue(DisplayDateProperty);
        set => SetValue(DisplayDateProperty, value);
    }

    public static readonly DependencyProperty DisplayDateStartProperty =
        DependencyProperty.Register(
            nameof(DisplayDateStart),
            typeof(DateTime?),
            typeof(NebulaCalendarView),
            new PropertyMetadata(null, OnCalendarStateChanged));

    public DateTime? DisplayDateStart
    {
        get => (DateTime?)GetValue(DisplayDateStartProperty);
        set => SetValue(DisplayDateStartProperty, value);
    }

    public static readonly DependencyProperty DisplayDateEndProperty =
        DependencyProperty.Register(
            nameof(DisplayDateEnd),
            typeof(DateTime?),
            typeof(NebulaCalendarView),
            new PropertyMetadata(null, OnCalendarStateChanged));

    public DateTime? DisplayDateEnd
    {
        get => (DateTime?)GetValue(DisplayDateEndProperty);
        set => SetValue(DisplayDateEndProperty, value);
    }

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(
            nameof(DisplayMode),
            typeof(NebulaCalendarMode),
            typeof(NebulaCalendarView),
            new PropertyMetadata(NebulaCalendarMode.Month, OnCalendarStateChanged));

    public NebulaCalendarMode DisplayMode
    {
        get => (NebulaCalendarMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public static readonly DependencyProperty HeaderTextProperty =
        DependencyProperty.Register(
            nameof(HeaderText),
            typeof(string),
            typeof(NebulaCalendarView),
            new PropertyMetadata(string.Empty));

    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        private set => SetValue(HeaderTextProperty, value);
    }

    public static readonly DependencyProperty ItemsColumnCountProperty =
        DependencyProperty.Register(
            nameof(ItemsColumnCount),
            typeof(int),
            typeof(NebulaCalendarView),
            new PropertyMetadata(7));

    public int ItemsColumnCount
    {
        get => (int)GetValue(ItemsColumnCountProperty);
        private set => SetValue(ItemsColumnCountProperty, value);
    }

    public static readonly DependencyProperty DayNamesVisibilityProperty =
        DependencyProperty.Register(
            nameof(DayNamesVisibility),
            typeof(Visibility),
            typeof(NebulaCalendarView),
            new PropertyMetadata(Visibility.Visible));

    public Visibility DayNamesVisibility
    {
        get => (Visibility)GetValue(DayNamesVisibilityProperty);
        private set => SetValue(DayNamesVisibilityProperty, value);
    }

    public ObservableCollection<NebulaCalendarItem> Items { get; }

    public ICommand PreviousCommand { get; }

    public ICommand NextCommand { get; }

    public ICommand HeaderCommand { get; }

    public ICommand SelectItemCommand => selectItemCommand;

    public event EventHandler? SelectedDateCommitted;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        RefreshItems();
    }

    private static void OnCalendarStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaCalendarView calendar)
        {
            calendar.RefreshItems();
        }
    }

    private void MovePrevious()
    {
        DisplayDate = DisplayMode switch
        {
            NebulaCalendarMode.Month => DisplayDate.AddMonths(-1),
            NebulaCalendarMode.Year => DisplayDate.AddYears(-1),
            _ => DisplayDate.AddYears(-10)
        };
    }

    private void MoveNext()
    {
        DisplayDate = DisplayMode switch
        {
            NebulaCalendarMode.Month => DisplayDate.AddMonths(1),
            NebulaCalendarMode.Year => DisplayDate.AddYears(1),
            _ => DisplayDate.AddYears(10)
        };
    }

    private void MoveUp()
    {
        DisplayMode = DisplayMode switch
        {
            NebulaCalendarMode.Month => NebulaCalendarMode.Year,
            NebulaCalendarMode.Year => NebulaCalendarMode.Decade,
            _ => NebulaCalendarMode.Decade
        };
    }

    private bool CanSelectItem(object? parameter)
    {
        return parameter is NebulaCalendarItem { IsEnabled: true };
    }

    private void SelectItem(object? parameter)
    {
        if (parameter is not NebulaCalendarItem item || !item.IsEnabled)
        {
            return;
        }

        switch (DisplayMode)
        {
            case NebulaCalendarMode.Month:
                SelectedDate = item.Date.Date;
                DisplayDate = item.Date.Date;
                SelectedDateCommitted?.Invoke(this, EventArgs.Empty);
                break;
            case NebulaCalendarMode.Year:
                DisplayDate = new DateTime(DisplayDate.Year, item.Date.Month, 1);
                DisplayMode = NebulaCalendarMode.Month;
                break;
            case NebulaCalendarMode.Decade:
                DisplayDate = new DateTime(item.Date.Year, DisplayDate.Month, 1);
                DisplayMode = NebulaCalendarMode.Year;
                break;
        }
    }

    private void RefreshItems()
    {
        HeaderText = DisplayMode switch
        {
            NebulaCalendarMode.Month => DisplayDate.ToString("MMMM yyyy", CultureInfo.CurrentCulture),
            NebulaCalendarMode.Year => DisplayDate.ToString("yyyy", CultureInfo.CurrentCulture),
            _ => $"{GetDecadeStart(DisplayDate.Year)}-{GetDecadeStart(DisplayDate.Year) + 9}"
        };
        ItemsColumnCount = DisplayMode == NebulaCalendarMode.Month ? 7 : 4;
        DayNamesVisibility = DisplayMode == NebulaCalendarMode.Month
            ? Visibility.Visible
            : Visibility.Collapsed;

        Items.Clear();

        switch (DisplayMode)
        {
            case NebulaCalendarMode.Month:
                BuildMonthItems();
                break;
            case NebulaCalendarMode.Year:
                BuildYearItems();
                break;
            case NebulaCalendarMode.Decade:
                BuildDecadeItems();
                break;
        }

        selectItemCommand.RaiseCanExecuteChanged();
    }

    private void BuildMonthItems()
    {
        var firstOfMonth = new DateTime(DisplayDate.Year, DisplayDate.Month, 1);
        var firstDayOffset = ((int)firstOfMonth.DayOfWeek - (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek + 7) % 7;
        var startDate = firstOfMonth.AddDays(-firstDayOffset);

        for (var i = 0; i < 42; i++)
        {
            var date = startDate.AddDays(i);
            Items.Add(new NebulaCalendarItem(
                date.Day.ToString(CultureInfo.CurrentCulture),
                date,
                date.Month == DisplayDate.Month,
                SelectedDate?.Date == date.Date,
                date.Date == DateTime.Today,
                IsInRange(date)));
        }
    }

    private void BuildYearItems()
    {
        for (var month = 1; month <= 12; month++)
        {
            var date = new DateTime(DisplayDate.Year, month, 1);
            Items.Add(new NebulaCalendarItem(
                CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[month - 1],
                date,
                true,
                SelectedDate?.Year == date.Year && SelectedDate?.Month == date.Month,
                date.Year == DateTime.Today.Year && date.Month == DateTime.Today.Month,
                IsMonthInRange(date)));
        }
    }

    private void BuildDecadeItems()
    {
        var startYear = GetDecadeStart(DisplayDate.Year);

        for (var i = -1; i < 11; i++)
        {
            var year = startYear + i;
            var date = new DateTime(year, 1, 1);
            Items.Add(new NebulaCalendarItem(
                year.ToString(CultureInfo.CurrentCulture),
                date,
                year >= startYear && year <= startYear + 9,
                SelectedDate?.Year == year,
                year == DateTime.Today.Year,
                IsYearInRange(year)));
        }
    }

    private bool IsInRange(DateTime date)
    {
        return (DisplayDateStart is null || date.Date >= DisplayDateStart.Value.Date)
            && (DisplayDateEnd is null || date.Date <= DisplayDateEnd.Value.Date);
    }

    private bool IsMonthInRange(DateTime month)
    {
        var monthStart = new DateTime(month.Year, month.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        return (DisplayDateStart is null || monthEnd >= DisplayDateStart.Value.Date)
            && (DisplayDateEnd is null || monthStart <= DisplayDateEnd.Value.Date);
    }

    private bool IsYearInRange(int year)
    {
        var yearStart = new DateTime(year, 1, 1);
        var yearEnd = new DateTime(year, 12, 31);
        return (DisplayDateStart is null || yearEnd >= DisplayDateStart.Value.Date)
            && (DisplayDateEnd is null || yearStart <= DisplayDateEnd.Value.Date);
    }

    private static int GetDecadeStart(int year)
    {
        return year - year % 10;
    }
}
