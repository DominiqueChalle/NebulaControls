using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaUI.Controls;

public class NebulaComboBox : ComboBox
{
    public static readonly DependencyProperty RejectInvalidTextProperty =
        DependencyProperty.Register(
            nameof(RejectInvalidText),
            typeof(bool),
            typeof(NebulaComboBox),
            new PropertyMetadata(true));

    public bool RejectInvalidText
    {
        get => (bool)GetValue(RejectInvalidTextProperty);
        set => SetValue(RejectInvalidTextProperty, value);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key == Key.Enter)
        {
            ValidateEditableText();
        }
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        ValidateEditableText();
        base.OnLostKeyboardFocus(e);
    }

    private void ValidateEditableText()
    {
        if (!IsEditable || !RejectInvalidText)
        {
            return;
        }

        var match = FindMatchingItem(Text);

        if (match is not null)
        {
            SelectedItem = match;
            Text = GetItemText(match);
            return;
        }

        Text = SelectedItem is null
            ? string.Empty
            : GetItemText(SelectedItem);
    }

    private object? FindMatchingItem(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var trimmedText = text.Trim();

        foreach (var item in Items)
        {
            if (item is ComboBoxItem { IsEnabled: false })
            {
                continue;
            }

            var itemText = GetItemText(item);

            if (string.Equals(itemText, trimmedText, StringComparison.CurrentCultureIgnoreCase))
            {
                return item;
            }
        }

        return null;
    }

    private static string GetItemText(object item)
    {
        if (item is ComboBoxItem comboBoxItem)
        {
            return comboBoxItem.Content?.ToString() ?? string.Empty;
        }

        var textSearchValue = item is DependencyObject dependencyObject
            ? TextSearch.GetText(dependencyObject)
            : string.Empty;

        return string.IsNullOrWhiteSpace(textSearchValue)
            ? item.ToString() ?? string.Empty
            : textSearchValue;
    }
}
