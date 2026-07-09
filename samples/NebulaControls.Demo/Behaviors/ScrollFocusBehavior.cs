using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace NebulaControls.Demo.Behaviors;

public static class ScrollFocusBehavior
{
    public static readonly DependencyProperty TrackKeyboardFocusProperty =
        DependencyProperty.RegisterAttached(
            "TrackKeyboardFocus",
            typeof(bool),
            typeof(ScrollFocusBehavior),
            new PropertyMetadata(false, OnTrackKeyboardFocusChanged));

    public static bool GetTrackKeyboardFocus(DependencyObject element)
    {
        return (bool)element.GetValue(TrackKeyboardFocusProperty);
    }

    public static void SetTrackKeyboardFocus(DependencyObject element, bool value)
    {
        element.SetValue(TrackKeyboardFocusProperty, value);
    }

    public static bool FocusFirstKeyboardTarget(DependencyObject root)
    {
        foreach (var element in GetFocusableDescendants(root))
        {
            if (element.Focus())
            {
                Keyboard.Focus(element);
                element.BringIntoView(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                return true;
            }
        }

        return false;
    }

    private static void OnTrackKeyboardFocusChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        if (element is not ScrollViewer scrollViewer)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            scrollViewer.AddHandler(
                Keyboard.GotKeyboardFocusEvent,
                new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus),
                true);
        }
        else
        {
            scrollViewer.RemoveHandler(
                Keyboard.GotKeyboardFocusEvent,
                new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
        }
    }

    private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer
            || e.NewFocus is not FrameworkElement focusedElement
            || !IsDescendantOf(focusedElement, scrollViewer))
        {
            return;
        }

        scrollViewer.Dispatcher.BeginInvoke(
            () => focusedElement.BringIntoView(new Rect(0, 0, focusedElement.ActualWidth, focusedElement.ActualHeight)),
            DispatcherPriority.Background);
    }

    private static bool IsDescendantOf(DependencyObject element, DependencyObject ancestor)
    {
        var current = element;

        while (current is not null)
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }

            current = GetParent(current);
        }

        return false;
    }

    private static DependencyObject? GetParent(DependencyObject element)
    {
        if (element is Visual or Visual3D)
        {
            return VisualTreeHelper.GetParent(element);
        }

        return LogicalTreeHelper.GetParent(element);
    }

    private static IEnumerable<FrameworkElement> GetFocusableDescendants(DependencyObject root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);

        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);

            if (child is FrameworkElement element && IsKeyboardNavigationTarget(element))
            {
                yield return element;
            }

            foreach (var descendant in GetFocusableDescendants(child))
            {
                yield return descendant;
            }
        }
    }

    private static bool IsKeyboardNavigationTarget(FrameworkElement element)
    {
        if (!element.Focusable || !element.IsVisible || !element.IsEnabled)
        {
            return false;
        }

        return element is not Control control || control.IsTabStop;
    }
}
