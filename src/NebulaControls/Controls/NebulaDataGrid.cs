// Nom: NebulaDataGrid
// Version: V1.04
// Description: DataGrid base control for editable Nebula data tables with optional assisted row validation.

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NebulaControls.Controls;

public class NebulaDataGrid : DataGrid
{
    private readonly Dictionary<object, HashSet<DataGridColumn>> requiredErrors = new(ReferenceEqualityComparer.Instance);

    public static readonly DependencyProperty KeepTabInsideGridProperty =
        DependencyProperty.Register(
            nameof(KeepTabInsideGrid),
            typeof(bool),
            typeof(NebulaDataGrid),
            new PropertyMetadata(false));

    public bool KeepTabInsideGrid
    {
        get => (bool)GetValue(KeepTabInsideGridProperty);
        set => SetValue(KeepTabInsideGridProperty, value);
    }

    public static readonly DependencyProperty DiscardEmptyNewRowsProperty =
        DependencyProperty.Register(
            nameof(DiscardEmptyNewRows),
            typeof(bool),
            typeof(NebulaDataGrid),
            new PropertyMetadata(false));

    public bool DiscardEmptyNewRows
    {
        get => (bool)GetValue(DiscardEmptyNewRowsProperty);
        set => SetValue(DiscardEmptyNewRowsProperty, value);
    }

    public static readonly DependencyProperty ValidateRequiredColumnsProperty =
        DependencyProperty.Register(
            nameof(ValidateRequiredColumns),
            typeof(bool),
            typeof(NebulaDataGrid),
            new PropertyMetadata(false));

    public bool ValidateRequiredColumns
    {
        get => (bool)GetValue(ValidateRequiredColumnsProperty);
        set => SetValue(ValidateRequiredColumnsProperty, value);
    }

    public static readonly DependencyProperty ContinueEditingNewRowsProperty =
        DependencyProperty.Register(
            nameof(ContinueEditingNewRows),
            typeof(bool),
            typeof(NebulaDataGrid),
            new PropertyMetadata(false));

    public bool ContinueEditingNewRows
    {
        get => (bool)GetValue(ContinueEditingNewRowsProperty);
        set => SetValue(ContinueEditingNewRowsProperty, value);
    }

    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.RegisterAttached(
            "IsRequired",
            typeof(bool),
            typeof(NebulaDataGrid),
            new PropertyMetadata(false));

    public static bool GetIsRequired(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsRequiredProperty);
    }

    public static void SetIsRequired(DependencyObject obj, bool value)
    {
        obj.SetValue(IsRequiredProperty, value);
    }

    public static readonly DependencyProperty HasRequiredErrorProperty =
        DependencyProperty.RegisterAttached(
            "HasRequiredError",
            typeof(bool),
            typeof(NebulaDataGrid),
            new PropertyMetadata(false));

    public static bool GetHasRequiredError(DependencyObject obj)
    {
        return (bool)obj.GetValue(HasRequiredErrorProperty);
    }

    public static void SetHasRequiredError(DependencyObject obj, bool value)
    {
        obj.SetValue(HasRequiredErrorProperty, value);
    }

    public bool ValidateRequiredCellsForCurrentRow(bool focusFirstError = true)
    {
        return ValidateRequiredCellsForItem(CurrentItem, focusFirstError);
    }

    public bool ValidateRequiredCellsForItem(object? item, bool focusFirstError = true)
    {
        if (!ValidateRequiredColumns || item is null || item == CollectionView.NewItemPlaceholder)
        {
            return true;
        }

        var firstMissingColumn = UpdateRequiredErrorState(item);
        if (firstMissingColumn is null)
        {
            return true;
        }

        if (focusFirstError)
        {
            MoveToCell(item, firstMissingColumn, beginEdit: true);
        }

        return false;
    }

    public bool ValidateRequiredCellsForItems(IEnumerable items, bool focusFirstError = true)
    {
        if (!ValidateRequiredColumns)
        {
            return true;
        }

        object? firstMissingItem = null;
        DataGridColumn? firstMissingColumn = null;

        foreach (var item in items.Cast<object>().Where(item => item != CollectionView.NewItemPlaceholder))
        {
            var missingColumn = UpdateRequiredErrorState(item);
            if (missingColumn is not null && firstMissingItem is null)
            {
                firstMissingItem = item;
                firstMissingColumn = missingColumn;
            }
        }

        if (firstMissingItem is null || firstMissingColumn is null)
        {
            return true;
        }

        if (focusFirstError)
        {
            MoveToCell(firstMissingItem, firstMissingColumn, beginEdit: true);
        }

        return false;
    }

    public bool HasMissingRequiredCells(object? item)
    {
        return ValidateRequiredColumns
            && item is not null
            && item != CollectionView.NewItemPlaceholder
            && HasMissingRequiredValue(item);
    }

    public void ClearRequiredErrorsForItem(object? item)
    {
        if (item is null)
        {
            return;
        }

        requiredErrors.Remove(item);

        foreach (var column in GetRequiredColumns())
        {
            ApplyRequiredErrorVisual(item, column, hasError: false);
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Tab && HandleTabNavigation())
        {
            e.Handled = true;
            return;
        }

        base.OnPreviewKeyDown(e);
    }

    protected override void OnLoadingRow(DataGridRowEventArgs e)
    {
        base.OnLoadingRow(e);

        Dispatcher.BeginInvoke(
            () => ApplyRequiredErrorVisuals(e.Row.Item),
            DispatcherPriority.Background);
    }

    private bool HandleTabNavigation()
    {
        if (CurrentColumn is null)
        {
            return false;
        }

        if (CurrentItem == CollectionView.NewItemPlaceholder)
        {
            return KeepTabInsideGrid && MoveToNewItemPlaceholder();
        }

        var currentIndex = Items.IndexOf(CurrentItem);
        var currentDisplayIndex = CurrentColumn.DisplayIndex;
        var movesBackward = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        var leavesCurrentRow = movesBackward
            ? !HasPreviousEditableColumn(currentDisplayIndex)
            : !HasNextEditableColumn(currentDisplayIndex);

        if (leavesCurrentRow && DiscardEmptyNewRows && IsEmptyNewItem(CurrentItem))
        {
            CommitEdit(DataGridEditingUnit.Cell, true);
            CommitEdit(DataGridEditingUnit.Row, true);
            RemoveItem(CurrentItem);
            return MoveToNewItemPlaceholder();
        }

        if (leavesCurrentRow && ValidateRequiredColumns && !ValidateRequiredCellsForItem(CurrentItem))
        {
            return true;
        }

        if (leavesCurrentRow && !movesBackward && ContinueEditingNewRows && IsItemNew(CurrentItem))
        {
            ClearRequiredErrorsForItem(CurrentItem);
            CommitEdit(DataGridEditingUnit.Cell, true);
            CommitEdit(DataGridEditingUnit.Row, true);

            Dispatcher.BeginInvoke(
                () => MoveToNewItemPlaceholder(beginEdit: true),
                DispatcherPriority.Background);

            return true;
        }

        if (!KeepTabInsideGrid)
        {
            return false;
        }

        CommitEdit(DataGridEditingUnit.Cell, true);
        CommitEdit(DataGridEditingUnit.Row, true);

        Dispatcher.BeginInvoke(
            () => MoveToEditableCell(currentIndex, currentDisplayIndex, movesBackward),
            DispatcherPriority.Background);

        return true;
    }

    private bool HasNextEditableColumn(int currentDisplayIndex)
    {
        return Columns.Any(column => !column.IsReadOnly && column.DisplayIndex > currentDisplayIndex);
    }

    private bool HasPreviousEditableColumn(int currentDisplayIndex)
    {
        return Columns.Any(column => !column.IsReadOnly && column.DisplayIndex < currentDisplayIndex);
    }

    private void MoveToEditableCell(int currentIndex, int currentDisplayIndex, bool movesBackward)
    {
        var editableColumns = GetEditableColumns();
        if (editableColumns.Length == 0 || Items.Count == 0)
        {
            return;
        }

        var nextColumn = movesBackward
            ? editableColumns.LastOrDefault(column => column.DisplayIndex < currentDisplayIndex)
            : editableColumns.FirstOrDefault(column => column.DisplayIndex > currentDisplayIndex);
        var nextIndex = currentIndex;

        if (nextColumn is null)
        {
            nextColumn = movesBackward ? editableColumns[^1] : editableColumns[0];
            nextIndex += movesBackward ? -1 : 1;
        }

        if (nextIndex >= Items.Count)
        {
            nextIndex = Items.Count - 1;
        }

        if (nextIndex < 0)
        {
            nextIndex = 0;
        }

        var nextItem = Items[nextIndex];
        if (nextItem == CollectionView.NewItemPlaceholder)
        {
            MoveToNewItemPlaceholder();
            return;
        }

        MoveToCell(nextItem, nextColumn, beginEdit: true);
    }

    private bool MoveToNewItemPlaceholder()
    {
        return MoveToNewItemPlaceholder(beginEdit: false);
    }

    private bool MoveToNewItemPlaceholder(bool beginEdit)
    {
        var column = GetEditableColumns().FirstOrDefault();
        if (column is null)
        {
            return false;
        }

        var placeholder = Items
            .Cast<object>()
            .FirstOrDefault(item => item == CollectionView.NewItemPlaceholder);

        if (placeholder is null)
        {
            return false;
        }

        MoveToCell(placeholder, column, beginEdit);
        return true;
    }

    private DataGridColumn? UpdateRequiredErrorState(object item)
    {
        DataGridColumn? firstMissingColumn = null;

        foreach (var column in GetRequiredColumns())
        {
            var hasError = IsEmptyValue(GetValue(item, column));
            SetRequiredError(item, column, hasError);

            if (hasError && firstMissingColumn is null)
            {
                firstMissingColumn = column;
            }
        }

        return firstMissingColumn;
    }

    private void MoveToCell(object item, DataGridColumn column, bool beginEdit)
    {
        Focus();
        SelectedItem = item;
        CurrentCell = new DataGridCellInfo(item, column);
        ScrollIntoView(item, column);

        Dispatcher.BeginInvoke(
            () =>
            {
                Focus();
                CurrentCell = new DataGridCellInfo(item, column);
                var cell = GetCell(item, column);
                if (cell is not null)
                {
                    SetHasRequiredError(cell, HasTrackedRequiredError(item, column));
                    cell.Focus();
                }

                if (beginEdit)
                {
                    BeginEdit();
                }
            },
            DispatcherPriority.Input);
    }

    private void SetRequiredError(object item, DataGridColumn column, bool hasError)
    {
        if (hasError)
        {
            if (!requiredErrors.TryGetValue(item, out var columns))
            {
                columns = [];
                requiredErrors[item] = columns;
            }

            columns.Add(column);
        }
        else if (requiredErrors.TryGetValue(item, out var columns))
        {
            columns.Remove(column);
            if (columns.Count == 0)
            {
                requiredErrors.Remove(item);
            }
        }

        ApplyRequiredErrorVisual(item, column, hasError);
    }

    private void ApplyRequiredErrorVisuals(object item)
    {
        foreach (var column in GetRequiredColumns())
        {
            ApplyRequiredErrorVisual(item, column, HasTrackedRequiredError(item, column));
        }
    }

    private void ApplyRequiredErrorVisual(object item, DataGridColumn column, bool hasError)
    {
        var cell = GetCell(item, column);
        if (cell is not null)
        {
            SetHasRequiredError(cell, hasError);
        }
    }

    private bool HasTrackedRequiredError(object item, DataGridColumn column)
    {
        return requiredErrors.TryGetValue(item, out var columns) && columns.Contains(column);
    }

    private DataGridCell? GetCell(object item, DataGridColumn column)
    {
        var row = ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
        if (row is null)
        {
            UpdateLayout();
            row = ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
        }

        if (row is null)
        {
            return null;
        }

        var presenter = FindVisualChild<DataGridCellsPresenter>(row);
        if (presenter is null)
        {
            row.ApplyTemplate();
            presenter = FindVisualChild<DataGridCellsPresenter>(row);
        }

        if (presenter?.ItemContainerGenerator.ContainerFromIndex(column.DisplayIndex) is DataGridCell cell)
        {
            return cell;
        }

        return null;
    }

    private static T? FindVisualChild<T>(DependencyObject source)
        where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(source); i++)
        {
            var child = VisualTreeHelper.GetChild(source, i);
            if (child is T match)
            {
                return match;
            }

            var descendant = FindVisualChild<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private DataGridColumn[] GetEditableColumns()
    {
        return Columns
            .Where(column => !column.IsReadOnly)
            .OrderBy(column => column.DisplayIndex)
            .ToArray();
    }

    private DataGridColumn[] GetRequiredColumns()
    {
        return Columns
            .Where(GetIsRequired)
            .OrderBy(column => column.DisplayIndex)
            .ToArray();
    }

    private bool HasMissingRequiredValue(object item)
    {
        return item != CollectionView.NewItemPlaceholder
            && GetRequiredColumns().Any(column => IsEmptyValue(GetValue(item, column)));
    }

    private bool IsEmptyNewItem(object item)
    {
        return item != CollectionView.NewItemPlaceholder
            && IsItemNew(item)
            && GetEditableColumns().All(column => IsEmptyValue(GetValue(item, column)));
    }

    private bool IsItemNew(object item)
    {
        var property = item.GetType().GetProperty("IsNew", BindingFlags.Instance | BindingFlags.Public);
        return property?.PropertyType == typeof(bool) && (bool)(property.GetValue(item) ?? false);
    }

    private static bool IsEmptyValue(object? value)
    {
        return value is null || value is string text && string.IsNullOrWhiteSpace(text);
    }

    private object? GetValue(object item, DataGridColumn column)
    {
        if (column is not DataGridBoundColumn boundColumn
            || boundColumn.Binding is not Binding binding
            || string.IsNullOrWhiteSpace(binding.Path?.Path))
        {
            return null;
        }

        var current = item;
        foreach (var part in binding.Path.Path.Split('.'))
        {
            var property = current.GetType().GetProperty(part, BindingFlags.Instance | BindingFlags.Public);
            if (property is null)
            {
                return null;
            }

            current = property.GetValue(current);
            if (current is null)
            {
                return null;
            }
        }

        return current;
    }

    private void RemoveItem(object item)
    {
        if (ItemsSource is IList list && list.Contains(item))
        {
            list.Remove(item);
        }
    }
}
