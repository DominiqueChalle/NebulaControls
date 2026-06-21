using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace NebulaControls.Demo.Views;

public partial class CollectionsDataView : UserControl
{
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
    }

    public ObservableCollection<DemoDataGridRow> DataGridRows { get; }

    public ObservableCollection<DemoDataGridRow> DisabledDataGridRows { get; }
}

public sealed record DemoDataGridRow(
    string Component,
    string Version,
    string Status,
    string Area,
    string Owner,
    string Priority,
    string Updated,
    string Notes);
