// Nom: NebulaSlider
// Version: V1.06
// Description: Slider base control styled for Nebula numeric range input.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaSlider : Slider
{
    public NebulaSlider()
    {
        AddHandler(MouseWheelEvent, new MouseWheelEventHandler(HandleMouseWheel), true);
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        if (!IsEnabled)
        {
            base.OnPreviewMouseWheel(e);
            return;
        }

        ChangeValueFromWheel(e.Delta);
        e.Handled = true;
    }

    private void HandleMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!IsEnabled || e.Handled)
        {
            return;
        }

        ChangeValueFromWheel(e.Delta);
        e.Handled = true;
    }

    private void ChangeValueFromWheel(int delta)
    {
        var direction = delta > 0 ? 1d : -1d;

        if (IsDirectionReversed)
        {
            direction *= -1d;
        }

        var nextValue = IsSnapToTickEnabled
            ? GetNextTickValue(direction)
            : GetNextSmallChangeValue(direction);
        SetCurrentValue(ValueProperty, nextValue);
    }

    private double GetNextSmallChangeValue(double direction)
    {
        var step = SmallChange > 0 ? SmallChange : 1d;
        var nextValue = Value + step * direction;
        return Math.Max(Minimum, Math.Min(Maximum, nextValue));
    }

    private double GetNextTickValue(double direction)
    {
        var ticks = GetOrderedTicks().ToList();
        if (ticks.Count == 0)
        {
            return GetNextSmallChangeValue(direction);
        }

        if (direction > 0)
        {
            return ticks.FirstOrDefault(tick => tick > Value, Maximum);
        }

        return ticks.LastOrDefault(tick => tick < Value, Minimum);
    }

    private IEnumerable<double> GetOrderedTicks()
    {
        if (Ticks is { Count: > 0 })
        {
            return Ticks
                .Cast<double>()
                .Where(tick => tick >= Minimum && tick <= Maximum)
                .Distinct()
                .OrderBy(tick => tick);
        }

        if (TickFrequency <= 0)
        {
            return [];
        }

        var ticks = new List<double>();
        for (var tick = Minimum; tick <= Maximum; tick += TickFrequency)
        {
            ticks.Add(Math.Max(Minimum, Math.Min(Maximum, tick)));
        }

        if (!ticks.Contains(Maximum))
        {
            ticks.Add(Maximum);
        }

        return ticks.Distinct().OrderBy(tick => tick);
    }
}
