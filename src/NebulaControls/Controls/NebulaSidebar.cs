// Nom: NebulaSidebar
// Version: V1.00
// Description: Collapsible sidebar navigation control with icon and text items.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NebulaControls.Controls;

public class NebulaSidebar : ListBox
{
    static NebulaSidebar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaSidebar),
            new FrameworkPropertyMetadata(typeof(NebulaSidebar)));
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(NebulaSidebar),
            new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(NebulaSidebar),
            new PropertyMetadata(string.Empty));

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.Register(
            nameof(IsCollapsed),
            typeof(bool),
            typeof(NebulaSidebar),
            new PropertyMetadata(false));

    public bool IsCollapsed
    {
        get => (bool)GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public static readonly DependencyProperty ExpandedWidthProperty =
        DependencyProperty.Register(
            nameof(ExpandedWidth),
            typeof(double),
            typeof(NebulaSidebar),
            new PropertyMetadata(244d));

    public double ExpandedWidth
    {
        get => (double)GetValue(ExpandedWidthProperty);
        set => SetValue(ExpandedWidthProperty, value);
    }

    public static readonly DependencyProperty CollapsedWidthProperty =
        DependencyProperty.Register(
            nameof(CollapsedWidth),
            typeof(double),
            typeof(NebulaSidebar),
            new PropertyMetadata(74d));

    public double CollapsedWidth
    {
        get => (double)GetValue(CollapsedWidthProperty);
        set => SetValue(CollapsedWidthProperty, value);
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new NebulaSidebarItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is NebulaSidebarItem;
    }

    public void Toggle()
    {
        IsCollapsed = !IsCollapsed;
    }
}

public class NebulaSidebarItem : ListBoxItem
{
    static NebulaSidebarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaSidebarItem),
            new FrameworkPropertyMetadata(typeof(NebulaSidebarItem)));
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(string),
            typeof(NebulaSidebarItem),
            new PropertyMetadata(string.Empty));

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}
