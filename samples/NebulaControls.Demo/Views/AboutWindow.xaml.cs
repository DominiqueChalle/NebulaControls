using NebulaControls.Controls;

namespace NebulaControls.Demo.Views;

public partial class AboutWindow : NebulaWindow
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Close();
    }
}
