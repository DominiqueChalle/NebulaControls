using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Data.Sqlite;
using NebulaControls.Controls;

namespace NebulaControls.Demo.Views;

public partial class CollectionsDataView : UserControl
{
    private readonly DemoDataGridRepository dataGridRepository = DemoDataGridRepository.CreateDataGridSandbox();
    private int deletedSandboxRowCount;
    private string selectedTreeViewItemText = "NebulaTreeView";

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

    private void DemoTreeView_SelectionCommitted(object sender, RoutedPropertyChangedEventArgs<object> e)
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

        NebulaDialog.ShowInfo(
            Window.GetWindow(this),
            "TreeView selection",
            $"Selected item: {selectedTreeViewItemText}");
    }

    private static string GetTreeViewHeaderText(TreeViewItem item)
    {
        return item.Header?.ToString() ?? "Tree item";
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

    private void ContextMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            ContextMenuResultText.Text = $"Context menu command: {item.Header}";
        }
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

    private void UpdateDataGridStatus()
    {
        var activeRows = DataGridRows.Where(row => !IsEmptyNewDataGridRow(row)).ToArray();
        var addedRows = activeRows.Count(row => row.IsNew);
        var changedRows = activeRows.Count(row => row.IsDirty || row.IsNew);
        var pendingRows = changedRows + deletedSandboxRowCount;
        DataGridRowCountText.Text = $"{DataGridRows.Count} row(s), {dataGridRepository.Count} persisted";
        DataGridAddStatusText.Text = pendingRows == 0
            ? "No pending record"
            : $"{pendingRows} pending record(s), {addedRows} new, {deletedSandboxRowCount} deleted";
        SaveDataGridChangesButton.IsEnabled = pendingRows > 0;
    }

    private void SaveDataGridChangesButton_Click(object sender, RoutedEventArgs e)
    {
        EditableDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        EditableDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
        if (CollectionViewSource.GetDefaultView(DataGridRows) is IEditableCollectionView editableView)
        {
            if (editableView.IsEditingItem)
            {
                editableView.CommitEdit();
            }

            if (editableView.IsAddingNew)
            {
                editableView.CommitNew();
            }
        }
        RemoveEmptyNewDataGridRows();

        var activeRows = DataGridRows
            .Where(row => !IsEmptyNewDataGridRow(row))
            .ToArray();
        if (!EditableDataGrid.ValidateRequiredCellsForItems(activeRows))
        {
            var invalidRow = activeRows.First(row => !CanSaveDataGridRow(row));
            invalidRow.ShowRequiredFieldErrors = true;
            ShowSaveBlockedDialog(invalidRow);
            UpdateDataGridStatus();
            return;
        }

        var validRows = activeRows
            .Where(CanSaveDataGridRow)
            .ToArray();
        var savedRows = validRows.Count(row => row.IsDirty || row.IsNew);
        var deletedRows = deletedSandboxRowCount;
        dataGridRepository.Save(validRows);
        deletedSandboxRowCount = 0;

        foreach (var row in validRows)
        {
            row.AcceptChanges();
        }

        UpdateDataGridStatus();
        DataGridSourceStatusText.Text = deletedRows == 0
            ? $"{savedRows} record(s) saved to SQLite."
            : $"{savedRows} record(s) saved to SQLite. {deletedRows} deleted.";
    }

    private void ReloadDataGridSourceButton_Click(object sender, RoutedEventArgs e)
    {
        LoadDataGridRowsFromSource("Reloaded from SQLite demo database.");
    }

    private void DeleteDataGridRowButton_Click(object sender, RoutedEventArgs e)
    {
        if (EditableDataGrid.SelectedItem is not DemoDataGridRow row)
        {
            DataGridSourceStatusText.Text = "Select a sandbox row to delete.";
            return;
        }

        if (EditableDataGrid.HasMissingRequiredCells(row))
        {
            row.ShowRequiredFieldErrors = true;
            EditableDataGrid.ValidateRequiredCellsForItem(row);

            var result = NebulaDialog.ShowModal(
                Window.GetWindow(this),
                "Incomplete row",
                $"This row is missing {GetMissingRequiredFieldsText(row)}. Do you really want to delete it?",
                NebulaDialogVariant.Warning,
                "Delete",
                "Modify",
                null,
                "DataGrid validation");

            if (result != NebulaDialogResult.Primary)
            {
                DataGridSourceStatusText.Text = $"Delete cancelled: complete {GetMissingRequiredFieldsText(row)} or choose another row.";
                return;
            }
        }

        if (row.Id != 0)
        {
            deletedSandboxRowCount++;
        }

        EditableDataGrid.CancelEdit(DataGridEditingUnit.Cell);
        EditableDataGrid.CancelEdit(DataGridEditingUnit.Row);
        EditableDataGrid.ClearRequiredErrorsForItem(row);
        EditableDataGrid.CurrentCell = default;
        EditableDataGrid.SelectedItem = null;

        DataGridRows.Remove(row);
        UpdateDataGridStatus();
        DataGridSourceStatusText.Text = row.Id == 0
            ? "Unsaved sandbox row removed."
            : $"Sandbox row {row.Id} marked for deletion. Save changes to persist.";
    }

    private void LoadDataGridRowsFromSource(string statusText)
    {
        deletedSandboxRowCount = 0;

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

    private void ShowSaveBlockedDialog(DemoDataGridRow row)
    {
        var missingFields = GetMissingRequiredFieldsText(row);
        DataGridSourceStatusText.Text = $"Save blocked: complete {missingFields}.";

        NebulaDialog.ShowModal(
            Window.GetWindow(this),
            "Incomplete row",
            $"Complete {missingFields} before saving the DataGrid changes.",
            NebulaDialogVariant.Warning,
            "Modify",
            "Cancel",
            null,
            "DataGrid validation");
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
    private const string InventoryTableName = "ControlRecords";
    private const string SandboxTableName = "DataGridSandboxRecords";

    private readonly string databasePath;
    private readonly string tableName;
    private int nextId;

    private DemoDataGridRepository(string tableName)
    {
        this.tableName = tableName;

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

    public static DemoDataGridRepository CreateControlInventory()
    {
        return new DemoDataGridRepository(InventoryTableName);
    }

    public static DemoDataGridRepository CreateDataGridSandbox()
    {
        return new DemoDataGridRepository(SandboxTableName);
    }

    public int Count
    {
        get
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {tableName};";
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }

    public IEnumerable<DemoDataGridRow> Load()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT Id, Component, Version, Status, Area, Owner, Priority, Updated, Notes
            FROM {tableName}
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
        deleteCommand.CommandText = $"DELETE FROM {tableName};";
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
            insertCommand.CommandText = $"""
                INSERT INTO {tableName}
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
        command.CommandText = $"""
            CREATE TABLE IF NOT EXISTS {tableName}
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
        var seedRows = tableName == InventoryTableName
            ? GetValidatedControlCatalog()
            : GetSandboxRows();

        Save(seedRows.Select(row => row.Clone()).ToArray());
    }

    public void EnsureControlCatalog()
    {
        if (tableName != InventoryTableName)
        {
            return;
        }

        Save(GetValidatedControlCatalog().Select(row => row.Clone()).ToArray());
    }

    private static DemoDataGridRow[] GetValidatedControlCatalog()
    {
        return
        [
            new(1001, "NebulaButtons", "v1.04", "Validated", "Actions", "Dominique", "High", "2026-07-15", "Primary, secondary, ghost, sizes and disabled states"),
            new(1002, "NebulaTextBox", "v1.03", "Validated", "Inputs", "Dominique", "High", "2026-07-15", "Generic text input with placeholder, helper text and validation states"),
            new(1003, "NebulaEmailTextBox", "v1.00", "Validated", "Inputs", "Dominique", "High", "2026-07-15", "Specialized email input with built-in format validation"),
            new(1004, "NebulaPasswordBox", "v1.04", "Validated", "Inputs", "Dominique", "High", "2026-07-23", "Password input styling and reveal behavior"),
            new(1005, "NebulaLabeledPasswordBox", "v1.03", "Validated", "Inputs", "Dominique", "High", "2026-07-23", "Labeled password input with helper, reveal and configurable rule validation"),
            new(1006, "NebulaDataGrid", "v1.04", "Validated", "Data", "Dominique", "High", "2026-07-15", "Editing, add-row, delete-row, required cells and SQLite sandbox scenario"),
            new(1007, "NebulaListBox", "v1.05", "Validated", "Collections", "Dominique", "Medium", "2026-07-15", "Keyboard navigation, disabled items and smooth item scrolling"),
            new(1008, "NebulaComboBox", "v1.06", "Validated", "Selection", "Dominique", "Medium", "2026-07-15", "Standard, editable, keyboard and disabled states"),
            new(1009, "NebulaTreeView", "v1.07", "Validated", "Collections", "Dominique", "Medium", "2026-07-24", "Hierarchy with keyboard navigation, collapse-safe selection and committed leaf selection"),
            new(1010, "NebulaTabControl", "v1.06", "Validated", "Layout", "Dominique", "Medium", "2026-07-15", "Top tab surface with keyboard navigation and disabled tab skipping"),
            new(1011, "NebulaAlert", "v1.02", "Validated", "Feedback", "Dominique", "High", "2026-07-15", "Interactive inline feedback"),
            new(1012, "NebulaDialog", "v1.05", "Validated", "Feedback", "Dominique", "High", "2026-07-15", "Modal dialog window with variants, results and custom content"),
            new(1013, "NebulaMessageBox", "v1.00", "Validated", "Feedback", "Dominique", "Medium", "2026-07-15", "MessageBox-style facade over NebulaDialog"),
            new(1014, "NebulaMenu", "v1.03", "Validated", "Navigation", "Dominique", "Medium", "2026-07-15", "Menu and context menu styling"),
            new(1015, "NebulaWindow", "v1.01", "Validated", "Shell", "Dominique", "High", "2026-07-15", "Custom Windows 11 shell with DWM rounded corners"),
            new(1016, "NebulaSidebar", "v1.00", "Validated", "Navigation", "Dominique", "High", "2026-07-15", "Collapsible navigation sidebar with icon and text items"),
            new(1017, "NebulaBadge", "v1.02", "Validated", "Feedback", "Dominique", "Medium", "2026-07-15", "Compact status labels and variants"),
            new(1018, "NebulaChip", "v1.03", "Validated", "Feedback", "Dominique", "Medium", "2026-07-15", "Selectable and removable filter tokens"),
            new(1019, "NebulaToast", "v1.04", "Validated", "Feedback", "Dominique", "High", "2026-07-15", "Stacked notifications with variants, timers and host API"),
            new(1020, "NebulaToastHost", "v1.01", "Validated", "Feedback", "Dominique", "High", "2026-07-15", "Toast host with typed show APIs and max visible toast count"),
            new(1021, "NebulaRating", "v1.03", "Validated", "Feedback", "Dominique", "Medium", "2026-07-15", "Interactive star rating with read-only and disabled states"),
            new(1022, "NebulaCheckBox", "v1.02", "Validated", "Selection", "Dominique", "Medium", "2026-07-15", "Checked, unchecked, indeterminate and disabled states"),
            new(1023, "NebulaRadioButton", "v1.02", "Validated", "Selection", "Dominique", "Medium", "2026-07-15", "Choice selection states"),
            new(1024, "NebulaToggleButton", "v1.03", "Validated", "Selection", "Dominique", "Medium", "2026-07-15", "Switch-style binary setting"),
            new(1025, "NebulaSlider", "v1.06", "Validated", "Inputs", "Dominique", "Medium", "2026-07-15", "Range input with keyboard and mouse wheel support"),
            new(1026, "NebulaProgressBar", "v1.03", "Validated", "Progress", "Dominique", "Medium", "2026-07-15", "Determinate and indeterminate progress indicator"),
            new(1027, "NebulaSpinner", "v1.02", "Validated", "Progress", "Dominique", "Low", "2026-07-15", "Indeterminate loading indicator"),
            new(1028, "NebulaSearchBox", "v1.02", "Validated", "Inputs", "Dominique", "High", "2026-07-15", "Search input with icon, clear action and submitted search event"),
            new(1029, "NebulaNumericUpDown", "v1.04", "Validated", "Inputs", "Dominique", "High", "2026-07-15", "Numeric input with buttons, keyboard, mouse wheel and range limits"),
            new(1030, "NebulaDatePicker", "v1.03", "Validated", "Pickers", "Dominique", "High", "2026-07-15", "Custom date picker with calendar popup and keyboard navigation"),
            new(1031, "NebulaTimePicker", "v1.03", "Validated", "Pickers", "Dominique", "High", "2026-07-15", "Clock-based time picker with manual entry and keyboard validation"),
            new(1032, "NebulaAvatar", "v1.02", "Validated", "Identity", "Dominique", "Medium", "2026-07-15", "Image avatar with neutral, male and female fallbacks"),
            new(1033, "NebulaExpander", "v1.03", "Validated", "Layout", "Dominique", "Medium", "2026-07-15", "Expandable content container with keyboard support"),
            new(1034, "NebulaGroupBox", "v1.02", "Validated", "Layout", "Dominique", "Medium", "2026-07-15", "Themed grouping container"),
            new(1035, "NebulaToolTip", "v1.02", "Validated", "Feedback", "Dominique", "Medium", "2026-07-15", "Simple and rich tooltip styling"),
            new(1036, "NebulaScrollBar", "v1.02", "Validated", "Navigation", "Dominique", "Medium", "2026-07-15", "Nebula scrollbar styling and item-by-item ListBox scroll")
        ];
    }

    private static DemoDataGridRow[] GetSandboxRows()
    {
        return
        [
            new(2001, "Customer profile", "V1", "Draft", "CRM", "Dominique", "High", "2026-07-06", "Editable sandbox row"),
            new(2002, "Invoice export", "V2", "Ready", "Billing", "Dominique", "Medium", "2026-07-06", "Try editing this row"),
            new(2003, "Notification rules", "V1", "Review", "Settings", "Dominique", "Low", "2026-07-06", "Try deleting this row")
        ];
    }

    private void RemoveInvalidRecords()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            DELETE FROM {tableName}
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
        command.CommandText = $"SELECT COALESCE(MAX(Id), 1000) + 1 FROM {tableName};";
        return Convert.ToInt32(command.ExecuteScalar());
    }
}
