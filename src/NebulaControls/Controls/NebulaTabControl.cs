// Nom: NebulaTabControl
// Version: V1.05
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
}

public class NebulaTabItem : TabItem
{
}
