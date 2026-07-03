using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Data.Sqlite;

namespace NebulaControls.Demo.Views;

public partial class CollectionsDataView : UserControl
{
    private readonly DemoDataGridRepository dataGridRepository = new();
    private string selectedTreeViewItemText = "NebulaTreeView";
    private bool treeViewClickStartedOnExpander;

    public CollectionsDataView()
    {
        DataGridRows = [];

        DisabledDataGridRows =
        [
            new(501, "Locked table", "V2", "Unavailable", "Data", "System", "Low", "Today", "Disabled preview"),
            new(502, "Read-only source", "V2", "Disabled", "Data", "System", "Low", "Today", "Disabled preview")
        ];

        InitializeComponent();

        DataGridRows.CollectionChanged += DataGridRows_CollectionChanged;
        LoadDataGridRowsFromSource("Loaded from SQLite demo database.");

        TreeViewSelectionText.Text = "Selected: NebulaTreeView";
    }

    public ObservableCollection<DemoDataGridRow> DataGridRows { get; }

    public ObservableCollection<DemoDataGridRow> DisabledDataGridRows { get; }

    private void DemoTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        treeViewClickStartedOnExpander = IsInsideTreeViewExpander(e.OriginalSource as DependencyObject);
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
        selectedTreeViewItemText = selectedText;
        TreeViewSelectionText.Text = $"Selected: {selectedText}";

        if (!treeViewClickStartedOnExpander)
        {
            MessageBox.Show(
                $"Selected item: {selectedTreeViewItemText}",
                "TreeView selection",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        treeViewClickStartedOnExpander = false;
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

    private static T? FindVisualParent<T>(DependencyObject? source)
        where T : DependencyObject
    {
        while (source is not null)
        {
            if (source is T match)
            {
                return match;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return null;
    }

    private void DataGridRows_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (DemoDataGridRow row in e.NewItems)
            {
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

    private void EditableDataGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid)
        {
            return;
        }

        var cell = FindVisualParent<DataGridCell>(e.OriginalSource as DependencyObject);
        if (cell is null || !cell.Column.IsReadOnly)
        {
            return;
        }

        var row = FindVisualParent<DataGridRow>(cell);
        if (row?.Item is null)
        {
            return;
        }

        var firstEditableColumn = dataGrid.Columns
            .Where(column => !column.IsReadOnly)
            .OrderBy(column => column.DisplayIndex)
            .FirstOrDefault();

        if (firstEditableColumn is null)
        {
            return;
        }

        e.Handled = true;
        dataGrid.SelectedItem = row.Item;
        dataGrid.CurrentCell = new DataGridCellInfo(row.Item, firstEditableColumn);
        dataGrid.ScrollIntoView(row.Item, firstEditableColumn);

        dataGrid.Dispatcher.BeginInvoke(
            () =>
            {
                dataGrid.Focus();
                dataGrid.CurrentCell = new DataGridCellInfo(row.Item, firstEditableColumn);
                dataGrid.BeginEdit();
            },
            DispatcherPriority.Input);
    }

    private void EditableDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab)
        {
            return;
        }

        if (sender is not DataGrid dataGrid || dataGrid.CurrentColumn is null)
        {
            return;
        }

        var currentIndex = dataGrid.Items.IndexOf(dataGrid.CurrentItem);
        var currentDisplayIndex = dataGrid.CurrentColumn.DisplayIndex;
        var movesBackward = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        var leavesCurrentRow = movesBackward
            ? !HasPreviousEditableColumn(dataGrid, currentDisplayIndex)
            : !HasNextEditableColumn(dataGrid, currentDisplayIndex);

        if (leavesCurrentRow
            && dataGrid.CurrentItem is DemoDataGridRow currentRow
            && !CanLeaveDataGridRow(currentRow))
        {
            currentRow.ShowRequiredFieldErrors = true;
            DataGridSourceStatusText.Text = $"Complete {GetMissingRequiredFieldsText(currentRow)} before leaving the new row.";
            e.Handled = true;
            MoveToFirstMissingRequiredCell(dataGrid, currentIndex, currentRow);
            return;
        }

        if (leavesCurrentRow
            && dataGrid.CurrentItem is DemoDataGridRow completedNewRow
            && completedNewRow.IsNew
            && CanSaveDataGridRow(completedNewRow))
        {
            dataGridRepository.AssignId(completedNewRow);
        }

        dataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        dataGrid.CommitEdit(DataGridEditingUnit.Row, true);

        e.Handled = true;

        Dispatcher.BeginInvoke(
            () =>
            {
                MoveToEditableDataGridCell(dataGrid, currentIndex, currentDisplayIndex, movesBackward);
                UpdateDataGridStatus();
            },
            DispatcherPriority.Background);
    }

    private static bool HasNextEditableColumn(DataGrid dataGrid, int currentDisplayIndex)
    {
        return dataGrid.Columns.Any(column => !column.IsReadOnly && column.DisplayIndex > currentDisplayIndex);
    }

    private static bool HasPreviousEditableColumn(DataGrid dataGrid, int currentDisplayIndex)
    {
        return dataGrid.Columns.Any(column => !column.IsReadOnly && column.DisplayIndex < currentDisplayIndex);
    }

    private static void MoveToEditableDataGridCell(DataGrid dataGrid, int currentIndex, int currentDisplayIndex, bool movesBackward)
    {
        if (dataGrid.Columns.Count == 0 || dataGrid.Items.Count == 0)
        {
            return;
        }

        var editableColumns = dataGrid.Columns
            .Where(column => !column.IsReadOnly)
            .OrderBy(column => column.DisplayIndex)
            .ToArray();

        if (editableColumns.Length == 0)
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

        if (nextIndex >= dataGrid.Items.Count)
        {
            nextIndex = dataGrid.Items.Count - 1;
        }

        if (nextIndex < 0)
        {
            nextIndex = 0;
        }

        var nextItem = dataGrid.Items[nextIndex];
        if (nextItem == CollectionView.NewItemPlaceholder && !dataGrid.CanUserAddRows)
        {
            return;
        }

        dataGrid.Focus();
        dataGrid.SelectedItem = nextItem;
        dataGrid.CurrentCell = new DataGridCellInfo(nextItem, nextColumn);
        dataGrid.ScrollIntoView(nextItem, nextColumn);

        dataGrid.Dispatcher.BeginInvoke(
            () =>
            {
                dataGrid.Focus();
                dataGrid.CurrentCell = new DataGridCellInfo(nextItem, nextColumn);
                dataGrid.BeginEdit();
            },
            DispatcherPriority.Input);
    }

    private void UpdateDataGridStatus()
    {
        var activeRows = DataGridRows.Where(row => !IsEmptyNewDataGridRow(row)).ToArray();
        var addedRows = activeRows.Count(row => row.IsNew);
        var pendingRows = activeRows.Count(row => row.IsDirty || row.IsNew);
        DataGridRowCountText.Text = $"{DataGridRows.Count} row(s), {dataGridRepository.Count} persisted";
        DataGridAddStatusText.Text = pendingRows == 0
            ? "No pending record"
            : $"{pendingRows} pending record(s), {addedRows} new";
        SaveDataGridChangesButton.IsEnabled = pendingRows > 0;
    }

    private void SaveDataGridChangesButton_Click(object sender, RoutedEventArgs e)
    {
        EditableDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        EditableDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
        RemoveEmptyNewDataGridRows();

        var activeRows = DataGridRows
            .Where(row => !IsEmptyNewDataGridRow(row))
            .ToArray();
        var invalidRows = activeRows
            .Where(row => !CanSaveDataGridRow(row))
            .ToArray();
        var invalidExistingRows = invalidRows
            .Where(row => !row.IsNew)
            .ToArray();

        if (invalidExistingRows.Length > 0)
        {
            invalidExistingRows[0].ShowRequiredFieldErrors = true;
            DataGridSourceStatusText.Text = $"Save blocked: complete {GetMissingRequiredFieldsText(invalidExistingRows[0])}.";
            MoveToFirstMissingRequiredCell(EditableDataGrid, DataGridRows.IndexOf(invalidExistingRows[0]), invalidExistingRows[0]);
            return;
        }

        var invalidNewRows = invalidRows
            .Where(row => row.IsNew)
            .ToArray();

        foreach (var row in invalidNewRows)
        {
            DataGridRows.Remove(row);
        }

        var validRows = activeRows
            .Where(CanSaveDataGridRow)
            .ToArray();
        var savedRows = validRows.Count(row => row.IsDirty || row.IsNew);
        dataGridRepository.Save(validRows);

        foreach (var row in validRows)
        {
            row.AcceptChanges();
        }

        UpdateDataGridStatus();
        DataGridSourceStatusText.Text = invalidNewRows.Length == 0
            ? $"{savedRows} record(s) saved to SQLite."
            : $"{savedRows} record(s) saved to SQLite. {invalidNewRows.Length} incomplete new row(s) discarded.";
    }

    private void ReloadDataGridSourceButton_Click(object sender, RoutedEventArgs e)
    {
        LoadDataGridRowsFromSource("Reloaded from SQLite demo database.");
    }

    private void LoadDataGridRowsFromSource(string statusText)
    {
        foreach (var row in DataGridRows)
        {
            row.PropertyChanged -= DataGridRow_PropertyChanged;
        }

        DataGridRows.Clear();

        foreach (var row in dataGridRepository.Load())
        {
            DataGridRows.Add(row);
        }

        UpdateDataGridStatus();
        DataGridSourceStatusText.Text = statusText;
    }

    private static bool CanLeaveDataGridRow(DemoDataGridRow row)
    {
        return !row.IsNew || IsEmptyNewDataGridRow(row) || CanSaveDataGridRow(row);
    }

    private static void MoveToFirstMissingRequiredCell(DataGrid dataGrid, int rowIndex, DemoDataGridRow row)
    {
        var targetColumn = GetFirstMissingRequiredColumn(dataGrid, row);
        if (targetColumn is null || rowIndex < 0)
        {
            dataGrid.BeginEdit();
            return;
        }

        var targetItem = dataGrid.Items[rowIndex];
        dataGrid.SelectedItem = targetItem;
        dataGrid.CurrentCell = new DataGridCellInfo(targetItem, targetColumn);
        dataGrid.ScrollIntoView(targetItem, targetColumn);

        dataGrid.Dispatcher.BeginInvoke(
            () =>
            {
                dataGrid.Focus();
                dataGrid.CurrentCell = new DataGridCellInfo(targetItem, targetColumn);
                dataGrid.BeginEdit();
            },
            DispatcherPriority.Input);
    }

    private static DataGridColumn? GetFirstMissingRequiredColumn(DataGrid dataGrid, DemoDataGridRow row)
    {
        var missingProperty = GetFirstMissingRequiredProperty(row);
        if (missingProperty is null)
        {
            return null;
        }

        return dataGrid.Columns
            .OfType<DataGridBoundColumn>()
            .FirstOrDefault(column =>
                column.Binding is Binding binding
                && binding.Path?.Path == missingProperty);
    }

    private static string? GetFirstMissingRequiredProperty(DemoDataGridRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Component))
        {
            return nameof(DemoDataGridRow.Component);
        }

        if (string.IsNullOrWhiteSpace(row.Version))
        {
            return nameof(DemoDataGridRow.Version);
        }

        if (string.IsNullOrWhiteSpace(row.Status))
        {
            return nameof(DemoDataGridRow.Status);
        }

        if (string.IsNullOrWhiteSpace(row.Area))
        {
            return nameof(DemoDataGridRow.Area);
        }

        return null;
    }

    private void RemoveEmptyNewDataGridRows()
    {
        var emptyRows = DataGridRows
            .Where(IsEmptyNewDataGridRow)
            .ToArray();

        foreach (var row in emptyRows)
        {
            DataGridRows.Remove(row);
        }
    }

    private static bool CanSaveDataGridRow(DemoDataGridRow row)
    {
        return !string.IsNullOrWhiteSpace(row.Component)
            && !string.IsNullOrWhiteSpace(row.Version)
            && !string.IsNullOrWhiteSpace(row.Status)
            && !string.IsNullOrWhiteSpace(row.Area);
    }

    private static string GetMissingRequiredFieldsText(DemoDataGridRow row)
    {
        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(row.Component))
        {
            missingFields.Add("Component");
        }

        if (string.IsNullOrWhiteSpace(row.Version))
        {
            missingFields.Add("Version");
        }

        if (string.IsNullOrWhiteSpace(row.Status))
        {
            missingFields.Add("Status");
        }

        if (string.IsNullOrWhiteSpace(row.Area))
        {
            missingFields.Add("Area");
        }

        return string.Join(", ", missingFields);
    }

    private static bool IsEmptyNewDataGridRow(DemoDataGridRow row)
    {
        return row.IsNew
            && string.IsNullOrWhiteSpace(row.Component)
            && string.IsNullOrWhiteSpace(row.Version)
            && string.IsNullOrWhiteSpace(row.Status)
            && string.IsNullOrWhiteSpace(row.Area)
            && string.IsNullOrWhiteSpace(row.Priority)
            && string.IsNullOrWhiteSpace(row.Notes);
    }
}

public sealed class DemoDataGridRow : INotifyPropertyChanged
{
    private string area = string.Empty;
    private string component = string.Empty;
    private int id;
    private bool isDirty;
    private bool isNew;
    private string notes = string.Empty;
    private string owner = string.Empty;
    private string priority = string.Empty;
    private bool showRequiredFieldErrors;
    private string status = string.Empty;
    private string updated = string.Empty;
    private string version = string.Empty;

    public DemoDataGridRow()
    {
        id = 0;
        component = string.Empty;
        version = string.Empty;
        status = string.Empty;
        area = string.Empty;
        owner = "Dominique";
        priority = string.Empty;
        updated = "Not saved";
        notes = string.Empty;
        isNew = true;
        isDirty = true;
    }

    public DemoDataGridRow(
        int id,
        string component,
        string version,
        string status,
        string area,
        string owner,
        string priority,
        string updated,
        string notes)
    {
        this.id = id;
        this.component = component;
        this.version = version;
        this.status = status;
        this.area = area;
        this.owner = owner;
        this.priority = priority;
        this.updated = updated;
        this.notes = notes;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Id
    {
        get => id;
        set => SetNumberValue(ref id, value);
    }

    public string Component
    {
        get => component;
        set => SetValue(ref component, value);
    }

    public string Version
    {
        get => version;
        set => SetValue(ref version, value);
    }

    public string Status
    {
        get => status;
        set => SetValue(ref status, value);
    }

    public string Area
    {
        get => area;
        set => SetValue(ref area, value);
    }

    public string Owner
    {
        get => owner;
        set => SetValue(ref owner, value);
    }

    public string Priority
    {
        get => priority;
        set => SetValue(ref priority, value);
    }

    public string Updated
    {
        get => updated;
        set => SetValue(ref updated, value);
    }

    public string Notes
    {
        get => notes;
        set => SetValue(ref notes, value);
    }

    public bool IsDirty
    {
        get => isDirty;
        set => SetFlag(ref isDirty, value);
    }

    public bool IsNew
    {
        get => isNew;
        set => SetFlag(ref isNew, value);
    }

    public bool ShowRequiredFieldErrors
    {
        get => showRequiredFieldErrors;
        set
        {
            if (showRequiredFieldErrors == value)
            {
                return;
            }

            showRequiredFieldErrors = value;
            OnPropertyChanged(nameof(ShowRequiredFieldErrors));
            OnRequiredFieldStateChanged();
        }
    }

    public void AcceptChanges()
    {
        ShowRequiredFieldErrors = false;
        IsNew = false;
        IsDirty = false;
        OnRequiredFieldStateChanged();
    }

    public void SetUpdatedFromRepository(string value)
    {
        if (updated == value)
        {
            return;
        }

        updated = value;
        OnPropertyChanged(nameof(Updated));
    }

    public bool HasRequiredFieldErrors
    {
        get
        {
            return (IsDirty || IsNew)
                && ShowRequiredFieldErrors
                && (!string.IsNullOrWhiteSpace(Component)
                    || !string.IsNullOrWhiteSpace(Version)
                    || !string.IsNullOrWhiteSpace(Status)
                    || !string.IsNullOrWhiteSpace(Area)
                    || !string.IsNullOrWhiteSpace(Priority)
                    || !string.IsNullOrWhiteSpace(Notes))
                && (string.IsNullOrWhiteSpace(Component)
                    || string.IsNullOrWhiteSpace(Version)
                    || string.IsNullOrWhiteSpace(Status)
                    || string.IsNullOrWhiteSpace(Area));
        }
    }

    public bool HasComponentError => HasRequiredFieldErrors && string.IsNullOrWhiteSpace(Component);

    public bool HasVersionError => HasRequiredFieldErrors && string.IsNullOrWhiteSpace(Version);

    public bool HasStatusError => HasRequiredFieldErrors && string.IsNullOrWhiteSpace(Status);

    public bool HasAreaError => HasRequiredFieldErrors && string.IsNullOrWhiteSpace(Area);

    public DemoDataGridRow Clone()
    {
        return new DemoDataGridRow(Id, Component, Version, Status, Area, Owner, Priority, Updated, Notes);
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
        OnRequiredFieldStateChanged();
    }

    private void SetNumberValue(ref int field, int value, [CallerMemberName] string? propertyName = null)
    {
        if (field == value)
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
        OnRequiredFieldStateChanged();
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

    private void OnRequiredFieldStateChanged()
    {
        OnPropertyChanged(nameof(HasRequiredFieldErrors));
        OnPropertyChanged(nameof(HasComponentError));
        OnPropertyChanged(nameof(HasVersionError));
        OnPropertyChanged(nameof(HasStatusError));
        OnPropertyChanged(nameof(HasAreaError));
    }
}

public sealed class DemoDataGridRepository
{
    private readonly string databasePath;
    private int nextId;

    public DemoDataGridRepository()
    {
        var dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NebulaControls",
            "Demo");

        Directory.CreateDirectory(dataFolder);
        databasePath = Path.Combine(dataFolder, "nebula-controls-demo.db");

        InitializeDatabase();
        RemoveInvalidRecords();
        nextId = GetNextId();
    }

    public int Count
    {
        get
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM ControlRecords;";
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }

    public IEnumerable<DemoDataGridRow> Load()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Component, Version, Status, Area, Owner, Priority, Updated, Notes
            FROM ControlRecords
            ORDER BY Id;
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return new DemoDataGridRow(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                reader.GetString(7),
                reader.GetString(8));
        }
    }

    public void AssignId(DemoDataGridRow row)
    {
        if (row.Id == 0)
        {
            row.Id = nextId++;
        }
    }

    public void Save(IEnumerable<DemoDataGridRow> rows)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        using var deleteCommand = connection.CreateCommand();
        deleteCommand.Transaction = transaction;
        deleteCommand.CommandText = "DELETE FROM ControlRecords;";
        deleteCommand.ExecuteNonQuery();

        foreach (var row in rows)
        {
            AssignId(row);

            if (row.IsDirty || row.IsNew)
            {
                row.SetUpdatedFromRepository(DateTime.Now.ToString("yyyy-MM-dd"));
            }

            using var insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = """
                INSERT INTO ControlRecords
                    (Id, Component, Version, Status, Area, Owner, Priority, Updated, Notes)
                VALUES
                    ($id, $component, $version, $status, $area, $owner, $priority, $updated, $notes);
                """;
            insertCommand.Parameters.AddWithValue("$id", row.Id);
            insertCommand.Parameters.AddWithValue("$component", row.Component);
            insertCommand.Parameters.AddWithValue("$version", row.Version);
            insertCommand.Parameters.AddWithValue("$status", row.Status);
            insertCommand.Parameters.AddWithValue("$area", row.Area);
            insertCommand.Parameters.AddWithValue("$owner", row.Owner);
            insertCommand.Parameters.AddWithValue("$priority", row.Priority);
            insertCommand.Parameters.AddWithValue("$updated", row.Updated);
            insertCommand.Parameters.AddWithValue("$notes", row.Notes);
            insertCommand.ExecuteNonQuery();
        }

        transaction.Commit();
        nextId = GetNextId();
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection($"Data Source={databasePath}");
        connection.Open();
        return connection;
    }

    private void InitializeDatabase()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS ControlRecords
            (
                Id INTEGER PRIMARY KEY,
                Component TEXT NOT NULL,
                Version TEXT NOT NULL,
                Status TEXT NOT NULL,
                Area TEXT NOT NULL,
                Owner TEXT NOT NULL,
                Priority TEXT NOT NULL,
                Updated TEXT NOT NULL,
                Notes TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();

        if (Count == 0)
        {
            SeedDatabase();
        }
    }

    private void SeedDatabase()
    {
        Save(
        [
            new(1001, "NebulaButton", "V2", "Validated", "Actions", "Dominique", "High", "2026-06-24", "Primary, secondary and ghost variants"),
            new(1002, "NebulaTextBox", "V2", "Validated", "Inputs", "Dominique", "High", "2026-06-24", "Placeholder, helper text and validation states"),
            new(1003, "NebulaPasswordBox", "V2", "Validated", "Inputs", "Dominique", "High", "2026-06-24", "Password reveal and validation sample"),
            new(1004, "NebulaDataGrid", "V2", "Reviewing", "Data", "Dominique", "High", "2026-07-03", "Editing, add-row and SQLite source scenario"),
            new(1005, "NebulaListBox", "V2", "Validated", "Collections", "Dominique", "Medium", "2026-06-25", "Smooth item-by-item scrolling"),
            new(1006, "NebulaComboBox", "V2", "Validated", "Selection", "Dominique", "Medium", "2026-06-25", "Standard, editable and disabled states"),
            new(1007, "NebulaTreeView", "V2", "Validated", "Collections", "Dominique", "Medium", "2026-06-29", "Hierarchy and disabled branch"),
            new(1008, "NebulaTabControl", "V2", "Validated", "Layout", "Dominique", "Low", "2026-06-29", "Accepted for V2, visual refinement later"),
            new(1009, "NebulaAlert", "V2", "Validated", "Feedback", "Dominique", "High", "2026-06-26", "Interactive inline feedback"),
            new(1010, "NebulaDialog", "V2", "Validated", "Feedback", "Dominique", "Medium", "2026-06-26", "Info, warning and danger dialogs"),
            new(1011, "NebulaMenu", "V2", "Validated", "Navigation", "Dominique", "Medium", "2026-06-26", "Menu and context menu hover states"),
            new(1012, "NebulaWindow", "V3", "Planned", "Shell", "Dominique", "Low", "Later", "Dedicated custom window control")
        ]);
    }

    private void RemoveInvalidRecords()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM ControlRecords
            WHERE TRIM(Component) = ''
               OR TRIM(Version) = ''
               OR TRIM(Status) = ''
               OR TRIM(Area) = '';
            """;
        command.ExecuteNonQuery();
    }

    private int GetNextId()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(MAX(Id), 1000) + 1 FROM ControlRecords;";
        return Convert.ToInt32(command.ExecuteScalar());
    }
}
