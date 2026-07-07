// Nom: NebulaTreeView
// Version: V1.03
// Description: TreeView base control for hierarchical Nebula navigation or selection with committed selection events.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NebulaControls.Controls;

public class NebulaTreeView : TreeView
{
    public static readonly RoutedEvent SelectionCommittedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SelectionCommitted),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<object>),
            typeof(NebulaTreeView));

    private bool selectionStartedOnExpander;

    public event RoutedPropertyChangedEventHandler<object> SelectionCommitted
    {
        add => AddHandler(SelectionCommittedEvent, value);
        remove => RemoveHandler(SelectionCommittedEvent, value);
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        selectionStartedOnExpander = IsInsideExpander(e.OriginalSource as DependencyObject);

        if (selectionStartedOnExpander)
        {
            Dispatcher.BeginInvoke(
                () => selectionStartedOnExpander = false,
                DispatcherPriority.Input);
        }

        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
    {
        base.OnSelectedItemChanged(e);

        if (selectionStartedOnExpander)
        {
            return;
        }

        RaiseEvent(new RoutedPropertyChangedEventArgs<object>(
            e.OldValue,
            e.NewValue,
            SelectionCommittedEvent));
    }

    private static bool IsInsideExpander(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is ToggleButton toggleButton && toggleButton.Name == "Expander")
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }
}
