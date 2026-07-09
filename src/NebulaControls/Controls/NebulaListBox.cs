// Nom: NebulaListBox
// Version: V1.03
// Description: ListBox base control for Nebula collection selection.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaListBox : ListBox
{
    private ScrollViewer? scrollViewer;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (IsNavigationKey(e.Key))
        {
            e.Handled = true;
        }
    }

    private static bool IsNavigationKey(Key key)
    {
        return key is Key.Up
            or Key.Down
            or Key.PageUp
            or Key.PageDown
            or Key.Home
            or Key.End;
    }
}
