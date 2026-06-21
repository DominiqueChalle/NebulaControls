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
}
