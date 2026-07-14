// Nom: NebulaTreeView
// Version: V1.06
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
    private bool commitSelectionOnNextChange;
    private ScrollViewer? scrollViewer;

    public NebulaTreeView()
    {
        AddHandler(MouseWheelEvent, new MouseWheelEventHandler(HandleMouseWheel), true);
    }

    public static readonly DependencyProperty CommitBranchItemsProperty =
        DependencyProperty.Register(
            nameof(CommitBranchItems),
            typeof(bool),
            typeof(NebulaTreeView),
            new PropertyMetadata(true));

    public bool CommitBranchItems
    {
        get => (bool)GetValue(CommitBranchItemsProperty);
        set => SetValue(CommitBranchItemsProperty, value);
    }

    public event RoutedPropertyChangedEventHandler<object> SelectionCommitted
    {
        add => AddHandler(SelectionCommittedEvent, value);
        remove => RemoveHandler(SelectionCommittedEvent, value);
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

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        selectionStartedOnExpander = IsInsideExpander(e.OriginalSource as DependencyObject);

        if (selectionStartedOnExpander)
        {
            Dispatcher.BeginInvoke(
                () => selectionStartedOnExpander = false,
                DispatcherPriority.Input);
        }
        else
        {
            commitSelectionOnNextChange = IsInsideTreeViewItem(e.OriginalSource as DependencyObject);
        }

        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key == Key.Enter && SelectedItem is not null)
        {
            var selectedItem = SelectedItem;
            if (CanCommitItem(selectedItem))
            {
                RaiseSelectionCommitted(selectedItem, selectedItem);
            }

            e.Handled = true;
            return;
        }

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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (IsNavigationKey(e.Key))
        {
            e.Handled = true;
        }
    }

    protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
    {
        base.OnSelectedItemChanged(e);

        if (selectionStartedOnExpander)
        {
            return;
        }

        if (commitSelectionOnNextChange)
        {
            if (e.NewValue is not null && CanCommitItem(e.NewValue))
            {
                RaiseSelectionCommitted(e.OldValue ?? e.NewValue, e.NewValue);
            }

            commitSelectionOnNextChange = false;
        }
    }

    private void RaiseSelectionCommitted(object oldValue, object newValue)
    {
        RaiseEvent(new RoutedPropertyChangedEventArgs<object>(
            oldValue,
            newValue,
            SelectionCommittedEvent));
    }

    private bool CanCommitItem(object item)
    {
        if (CommitBranchItems)
        {
            return true;
        }

        var treeViewItem = item as TreeViewItem
            ?? ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

        return treeViewItem is null
            || !treeViewItem.HasItems;
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

    private static bool IsInsideTreeViewItem(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is TreeViewItem)
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private static bool IsNavigationKey(Key key)
    {
        return key is Key.Up
            or Key.Down
            or Key.Left
            or Key.Right
            or Key.PageUp
            or Key.PageDown
            or Key.Home
            or Key.End;
    }
}
