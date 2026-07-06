using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace NebulaControls.Demo.Views;

public partial class ControlInventoryView : UserControl
{
    private readonly DemoDataGridRepository inventoryRepository = DemoDataGridRepository.CreateControlInventory();

    public ControlInventoryView()
    {
        InventoryRows = [];
        InitializeComponent();
        LoadInventory();
    }

    public ObservableCollection<DemoDataGridRow> InventoryRows { get; }

    private void LoadInventory()
    {
        inventoryRepository.EnsureControlCatalog();

        InventoryRows.Clear();
        foreach (var row in inventoryRepository.Load())
        {
            InventoryRows.Add(row);
        }

        InventoryStatusText.Text = $"{InventoryRows.Count} validated control record(s).";
    }
}
