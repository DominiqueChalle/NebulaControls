// Nom: NebulaTabControl
// Version: V1.06
// Description: TabControl and TabItem base controls styled for Nebula top tab navigation with binder-style depth.

using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace NebulaControls.Controls;

public class NebulaTabControl : TabControl
{
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new NebulaTabItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is NebulaTabItem;
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateTabDepthAsync();
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        UpdateTabDepth();
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);

        if (e.NewFocus == this)
        {
            FocusSelectedTab();
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key is Key.Left or Key.Up)
        {
            SelectRelativeTab(-1);
            e.Handled = true;
            return;
        }

        if (e.Key is Key.Right or Key.Down)
        {
            SelectRelativeTab(1);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Home)
        {
            SelectBoundaryTab(0, 1);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.End)
        {
            SelectBoundaryTab(Items.Count - 1, -1);
            e.Handled = true;
            return;
        }

        base.OnPreviewKeyDown(e);
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        UpdateTabDepthAsync();
    }

    private void UpdateTabDepthAsync()
    {
        Dispatcher.BeginInvoke(UpdateTabDepth, DispatcherPriority.Loaded);
    }

    private void UpdateTabDepth()
    {
        var count = Items.Count;

        for (var index = 0; index < count; index++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(index) is not TabItem tabItem)
            {
                continue;
            }

            var depth = tabItem.IsSelected
                ? count + 10
                : count - index;

            Panel.SetZIndex(tabItem, depth);
        }
    }

    private void FocusSelectedTab()
    {
        if (ItemContainerGenerator.ContainerFromIndex(SelectedIndex) is TabItem tabItem && tabItem.IsEnabled)
        {
            tabItem.Focus();
        }
    }

    private void SelectRelativeTab(int direction)
    {
        if (Items.Count == 0)
        {
            return;
        }

        var startIndex = SelectedIndex < 0 ? 0 : SelectedIndex;

        for (var offset = 1; offset <= Items.Count; offset++)
        {
            var candidateIndex = (startIndex + direction * offset + Items.Count) % Items.Count;
            if (TrySelectTab(candidateIndex))
            {
                return;
            }
        }
    }

    private void SelectBoundaryTab(int startIndex, int direction)
    {
        for (var index = startIndex; index >= 0 && index < Items.Count; index += direction)
        {
            if (TrySelectTab(index))
            {
                return;
            }
        }
    }

    private bool TrySelectTab(int index)
    {
        if (ItemContainerGenerator.ContainerFromIndex(index) is not TabItem tabItem || !tabItem.IsEnabled)
        {
            return false;
        }

        SelectedIndex = index;
        tabItem.Focus();
        UpdateTabDepth();
        return true;
    }
}

public class NebulaTabItem : TabItem
{
}
