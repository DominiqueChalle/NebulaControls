// Nom: NebulaListBox
// Version: V1.05
// Description: ListBox base control for Nebula collection selection.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaListBox : ListBox
{
    private ScrollViewer? scrollViewer;

    public NebulaListBox()
    {
        AddHandler(MouseWheelEvent, new MouseWheelEventHandler(HandleMouseWheel), true);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        ScrollFromWheel(e);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (TryHandleNavigationKey(e.Key))
        {
            e.Handled = true;
            return;
        }

        base.OnPreviewKeyDown(e);
    }

    private void HandleMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        ScrollFromWheel(e);
    }

    private void ScrollFromWheel(MouseWheelEventArgs e)
    {
        if (scrollViewer is null)
        {
            base.OnPreviewMouseWheel(e);
            return;
        }

        if (e.Delta < 0)
        {
            scrollViewer.LineDown();
        }
        else
        {
            scrollViewer.LineUp();
        }

        e.Handled = true;
    }

    private bool TryHandleNavigationKey(Key key)
    {
        return key switch
        {
            Key.Up => SelectRelativeItem(-1),
            Key.Down => SelectRelativeItem(1),
            Key.PageUp => SelectRelativeItem(-GetPageStep()),
            Key.PageDown => SelectRelativeItem(GetPageStep()),
            Key.Home => SelectBoundaryItem(0, 1),
            Key.End => SelectBoundaryItem(Items.Count - 1, -1),
            _ => false
        };
    }

    private int GetPageStep()
    {
        if (scrollViewer is null)
        {
            return 5;
        }

        var itemHeight = GetAverageItemHeight();
        var visibleItems = itemHeight <= 0
            ? 5
            : Math.Max(1, (int)Math.Floor(scrollViewer.ViewportHeight / itemHeight));

        return Math.Max(1, visibleItems - 1);
    }

    private double GetAverageItemHeight()
    {
        var totalHeight = 0d;
        var measuredItems = 0;

        for (var index = 0; index < Items.Count; index++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(index) is not ListBoxItem { ActualHeight: > 0 } item)
            {
                continue;
            }

            totalHeight += item.ActualHeight;
            measuredItems++;
        }

        return measuredItems == 0
            ? 0
            : totalHeight / measuredItems;
    }

    private bool SelectRelativeItem(int offset)
    {
        if (Items.Count == 0)
        {
            return false;
        }

        var direction = offset < 0 ? -1 : 1;
        var remainingSteps = int.Abs(offset);
        var index = SelectedIndex < 0
            ? (direction > 0 ? -1 : Items.Count)
            : SelectedIndex;
        var moved = false;

        while (remainingSteps > 0)
        {
            var nextIndex = FindNextSelectableIndex(index, direction);
            if (nextIndex < 0)
            {
                return moved
                    ? TrySelectItem(index)
                    : true;
            }

            index = nextIndex;
            moved = true;
            remainingSteps--;
        }

        return TrySelectItem(index);
    }

    private bool SelectBoundaryItem(int startIndex, int direction)
    {
        var index = startIndex;
        while (index >= 0 && index < Items.Count)
        {
            if (TrySelectItem(index))
            {
                return true;
            }

            index += direction;
        }

        return true;
    }

    private int FindNextSelectableIndex(int currentIndex, int direction)
    {
        var index = currentIndex + direction;
        while (index >= 0 && index < Items.Count)
        {
            if (IsSelectableIndex(index))
            {
                return index;
            }

            index += direction;
        }

        return -1;
    }

    private bool TrySelectItem(int index)
    {
        if (!IsSelectableIndex(index))
        {
            return false;
        }

        SelectedIndex = index;
        ScrollIntoView(Items[index]);

        if (ItemContainerGenerator.ContainerFromIndex(index) is ListBoxItem item)
        {
            item.Focus();
        }

        return true;
    }

    private bool IsSelectableIndex(int index)
    {
        if (index < 0 || index >= Items.Count)
        {
            return false;
        }

        return ItemContainerGenerator.ContainerFromIndex(index) is not ListBoxItem item
            || item.IsEnabled;
    }
}
