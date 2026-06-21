using System;

namespace NebulaControls.Controls;

public sealed class NebulaCalendarItem
{
    public NebulaCalendarItem(
        string label,
        DateTime date,
        bool isCurrentScope,
        bool isSelected,
        bool isToday,
        bool isEnabled)
    {
        Label = label;
        Date = date;
        IsCurrentScope = isCurrentScope;
        IsSelected = isSelected;
        IsToday = isToday;
        IsEnabled = isEnabled;
    }

    public string Label { get; }

    public DateTime Date { get; }

    public bool IsCurrentScope { get; }

    public bool IsSelected { get; }

    public bool IsToday { get; }

    public bool IsEnabled { get; }
}
