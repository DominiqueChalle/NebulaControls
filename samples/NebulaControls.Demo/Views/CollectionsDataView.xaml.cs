using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NebulaControls.Demo.Views;

public partial class CollectionsDataView : UserControl
{
    private int _initialDataGridRowCount;
    private string _selectedTreeViewItemText = "NebulaTreeView";
    private bool _treeViewClickStartedOnExpander;

    public CollectionsDataView()
    {
        DataGridRows = new ObservableCollection<DemoDataGridRow>
        {
            new("NebulaButton", "V2", "Validated", "Actions", "Dominique", "High", "Today", "Primary, secondary and ghost variants"),
            new("NebulaTextBox", "V2", "Validated", "Inputs", "Dominique", "High", "Today", "Placeholder, helper text and validation states"),
            new("NebulaPasswordBox", "V2", "Validated", "Inputs", "Dominique", "High", "Today", "Password reveal and validation sample"),
            new("NebulaDataGrid", "V2", "Reviewing", "Data", "Dominique", "High", "Today", "Horizontal scroll and column resize"),
            new("NebulaListBox", "V2", "Validated", "Collections", "Dominique", "Medium", "Yesterday", "Smooth item-by-item scrolling"),
            new("NebulaComboBox", "V2", "Validated", "Selection", "Dominique", "Medium", "Yesterday", "Standard, editable and disabled states"),
            new("NebulaTreeView", "V2", "Reviewing", "Collections", "Dominique", "Medium", "This week", "Hierarchy and disabled branch"),
            new("NebulaTabControl", "V2", "Validated", "Layout", "Dominique", "Low", "This week", "Accepted for V2, visual refinement later"),
            new("NebulaAlert", "V2", "Validated", "Feedback", "Dominique", "High", "This week", "Interactive inline feedback"),
            new("NebulaDialog", "V2", "Validated", "Feedback", "Dominique", "Medium", "This week", "Info, warning and danger dialogs"),
            new("NebulaMenu", "V2", "Validated", "Navigation", "Dominique", "Medium", "This week", "Menu and context menu hover states"),
            new("NebulaWindow", "V3", "Planned", "Shell", "Dominique", "Low", "Later", "Dedicated custom window control")
        };

        DisabledDataGridRows = new ObservableCollection<DemoDataGridRow>
        {
            new("Locked table", "V2", "Unavailable", "Data", "System", "Low", "Today", "Disabled preview"),
            new("Read-only source", "V2", "Disabled", "Data", "System", "Low", "Today", "Disabled preview")
        };

        InitializeComponent();

        _initialDataGridRowCount = DataGridRows.Count;
        DataGridRows.CollectionChanged += DataGridRows_CollectionChanged;
        foreach (var row in DataGridRows)
        {
            row.PropertyChanged += DataGridRow_PropertyChanged;
        }

        TreeViewSelectionText.Text = "Selected: NebulaTreeView";
        UpdateDataGridStatus();
    }

    public ObservableCollection<DemoDataGridRow> DataGridRows { get; }

    public ObservableCollection<DemoDataGridRow> DisabledDataGridRows { get; }

    private void DemoTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _treeViewClickStartedOnExpander = IsInsideTreeViewExpander(e.OriginalSource as DependencyObject);
    }

    private void DemoTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (TreeViewSelectionText is null)
        {
            return;
        }

        if (e.NewValue is not TreeViewItem item)
        {
            TreeViewSelectionText.Text = "Selected: none";
            return;
        }

        var selectedText = GetTreeViewHeaderText(item);
        _selectedTreeViewItemText = selectedText;
        TreeViewSelectionText.Text = $"Selected: {selectedText}";

        if (!_treeViewClickStartedOnExpander)
        {
            MessageBox.Show(
                $"Selected item: {_selectedTreeViewItemText}",
                "TreeView selection",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        _treeViewClickStartedOnExpander = false;
    }

    private static string GetTreeViewHeaderText(TreeViewItem item)
    {
        return item.Header?.ToString() ?? "Tree item";
    }

    private static bool IsInsideTreeViewExpander(DependencyObject? source)
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

    private void DataGridRows_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (DemoDataGridRow row in e.NewItems)
            {
                row.IsNew = true;
                row.IsDirty = true;
                row.PropertyChanged += DataGridRow_PropertyChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (DemoDataGridRow row in e.OldItems)
            {
                row.PropertyChanged -= DataGridRow_PropertyChanged;
            }
        }

        UpdateDataGridStatus();
    }

    private void DataGridRow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DemoDataGridRow.IsDirty) or nameof(DemoDataGridRow.IsNew))
        {
            UpdateDataGridStatus();
        }
    }

    private void EditableDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        Dispatcher.BeginInvoke(UpdateDataGridStatus, DispatcherPriority.Background);
    }

    private void EditableDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            return;
        }

        if (sender is not DataGrid dataGrid || dataGrid.CurrentColumn is null)
        {
            return;
        }

        if (dataGrid.CurrentColumn.DisplayIndex != dataGrid.Columns.Count - 1)
        {
            return;
        }

        var currentIndex = dataGrid.Items.IndexOf(dataGrid.CurrentItem);

        dataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        dataGrid.CommitEdit(DataGridEditingUnit.Row, true);

        e.Handled = true;

        Dispatcher.BeginInvoke(
            () =>
            {
                MoveToNextDataGridRow(dataGrid, currentIndex);
                UpdateDataGridStatus();
            },
            DispatcherPriority.Background);
    }

    private static void MoveToNextDataGridRow(DataGrid dataGrid, int currentIndex)
    {
        if (dataGrid.Columns.Count == 0 || dataGrid.Items.Count == 0)
        {
            return;
        }

        var nextIndex = currentIndex + 1;
        if (nextIndex < 0 || nextIndex >= dataGrid.Items.Count)
        {
            nextIndex = dataGrid.Items.Count - 1;
        }

        var nextItem = dataGrid.Items[nextIndex];
        if (nextItem == CollectionView.NewItemPlaceholder && !dataGrid.CanUserAddRows)
        {
            return;
        }

        var firstColumn = dataGrid.Columns[0];
        dataGrid.SelectedItem = nextItem;
        dataGrid.CurrentCell = new DataGridCellInfo(nextItem, firstColumn);
        dataGrid.ScrollIntoView(nextItem, firstColumn);
        dataGrid.BeginEdit();
    }

    private void UpdateDataGridStatus()
    {
        var addedRows = Math.Max(0, DataGridRows.Count - _initialDataGridRowCount);
        var pendingRows = DataGridRows.Count(row => row.IsDirty || row.IsNew);
        DataGridRowCountText.Text = $"{DataGridRows.Count} row(s) in collection";
        DataGridAddStatusText.Text = pendingRows == 0
            ? "No pending record"
            : $"{pendingRows} pending record(s), {addedRows} new";
        SaveDataGridChangesButton.IsEnabled = pendingRows > 0;
    }

    private void SaveDataGridChangesButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        foreach (var row in DataGridRows)
        {
            row.AcceptChanges();
        }

        _initialDataGridRowCount = DataGridRows.Count;
        UpdateDataGridStatus();
        DataGridAddStatusText.Text = "Changes saved to the demo data source";
    }
}

public sealed class DemoDataGridRow : INotifyPropertyChanged
{
    private string _component = string.Empty;
    private bool _isDirty;
    private bool _isNew;
    private string _version = string.Empty;
    private string _status = string.Empty;
    private string _area = string.Empty;
    private string _owner = string.Empty;
    private string _priority = string.Empty;
    private string _updated = string.Empty;
    private string _notes = string.Empty;

    public DemoDataGridRow()
    {
        _component = "New component";
        _version = "V2";
        _status = "Draft";
        _area = "Data";
        _owner = "Dominique";
        _priority = "Medium";
        _updated = "Today";
        _notes = "New editable row";
        _isNew = true;
        _isDirty = true;
    }

    public DemoDataGridRow(
        string component,
        string version,
        string status,
        string area,
        string owner,
        string priority,
        string updated,
        string notes)
    {
        _component = component;
        _version = version;
        _status = status;
        _area = area;
        _owner = owner;
        _priority = priority;
        _updated = updated;
        _notes = notes;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Component
    {
        get => _component;
        set => SetValue(ref _component, value);
    }

    public string Version
    {
        get => _version;
        set => SetValue(ref _version, value);
    }

    public string Status
    {
        get => _status;
        set => SetValue(ref _status, value);
    }

    public string Area
    {
        get => _area;
        set => SetValue(ref _area, value);
    }

    public string Owner
    {
        get => _owner;
        set => SetValue(ref _owner, value);
    }

    public string Priority
    {
        get => _priority;
        set => SetValue(ref _priority, value);
    }

    public string Updated
    {
        get => _updated;
        set => SetValue(ref _updated, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetValue(ref _notes, value);
    }

    public bool IsDirty
    {
        get => _isDirty;
        set => SetFlag(ref _isDirty, value);
    }

    public bool IsNew
    {
        get => _isNew;
        set => SetFlag(ref _isNew, value);
    }

    public void AcceptChanges()
    {
        IsNew = false;
        IsDirty = false;
    }

    private void SetValue(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (field == value)
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
        IsDirty = true;
    }

    private void SetFlag(ref bool field, bool value, [CallerMemberName] string? propertyName = null)
    {
        if (field == value)
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
