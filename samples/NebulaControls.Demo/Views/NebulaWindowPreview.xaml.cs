using NebulaControls.Controls;

namespace NebulaControls.Demo.Views;

public partial class NebulaWindowPreview : NebulaWindow
{
    public NebulaWindowPreview()
    {
        InitializeComponent();
    }

    private void ClosePreviewButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Close();
    }
}
