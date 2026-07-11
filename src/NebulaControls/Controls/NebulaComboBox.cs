// Nom: NebulaComboBox
// Version: V1.06
// Description: ComboBox base control used by Nebula selection inputs.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaComboBox : ComboBox
{
    private bool _skipNextEditableValidation;
    private bool _hasRestoreSnapshot;
    private object? _restoreSelectedItem;
    private string _restoreText = string.Empty;

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
        if ((e.Key == Key.Down || e.Key == Key.Up) && !IsDropDownOpen)
        {
            IsDropDownOpen = true;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter)
        {
            if (!IsDropDownOpen)
            {
                CaptureRestoreSnapshot();
                IsDropDownOpen = true;
                e.Handled = true;
                return;
            }

            if (!IsEditable)
            {
                CommitHighlightedItem();
                IsDropDownOpen = false;
                e.Handled = true;
                return;
            }

            ValidateEditableText();
            IsDropDownOpen = false;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape && IsDropDownOpen)
        {
            if (IsEditable)
            {
                RestoreSnapshot();
            }

            _skipNextEditableValidation = true;
            IsDropDownOpen = false;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape && IsEditable)
        {
            RestoreSnapshot();
            _skipNextEditableValidation = true;
            IsDropDownOpen = false;
            e.Handled = true;
            return;
        }

        base.OnPreviewKeyDown(e);
    }

    protected override void OnDropDownOpened(EventArgs e)
    {
        CaptureRestoreSnapshot();
        base.OnDropDownOpened(e);
    }

    protected override void OnDropDownClosed(EventArgs e)
    {
        if (_skipNextEditableValidation)
        {
            _skipNextEditableValidation = false;
        }
        else
        {
            ValidateEditableText();
        }

        ClearRestoreSnapshot();
        base.OnDropDownClosed(e);
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        ValidateEditableText();
        base.OnLostKeyboardFocus(e);
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        CaptureRestoreSnapshot();
        base.OnGotKeyboardFocus(e);
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

    private void RestoreSelectedText()
    {
        Text = SelectedItem is null
            ? string.Empty
            : GetItemText(SelectedItem);
    }

    private void CaptureRestoreSnapshot()
    {
        if (!IsEditable || _hasRestoreSnapshot)
        {
            return;
        }

        _restoreSelectedItem = SelectedItem;
        _restoreText = Text;
        _hasRestoreSnapshot = true;
    }

    private void RestoreSnapshot()
    {
        if (!_hasRestoreSnapshot)
        {
            RestoreSelectedText();
            return;
        }

        SelectedItem = _restoreSelectedItem;
        Text = _restoreText;
    }

    private void ClearRestoreSnapshot()
    {
        _restoreSelectedItem = null;
        _restoreText = string.Empty;
        _hasRestoreSnapshot = false;
    }

    private void CommitHighlightedItem()
    {
        foreach (var item in Items)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is ComboBoxItem { IsHighlighted: true, IsEnabled: true })
            {
                SelectedItem = item;
                return;
            }
        }
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
