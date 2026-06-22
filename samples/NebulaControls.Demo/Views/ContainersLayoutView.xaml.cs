using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace NebulaControls.Demo.Views;

public partial class ContainersLayoutView : UserControl
{
    public ContainersLayoutView()
    {
        InitializeComponent();
    }

    private void LoadAvatarButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select an avatar image",
            Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var avatarImage = new BitmapImage();
        avatarImage.BeginInit();
        avatarImage.CacheOption = BitmapCacheOption.OnLoad;
        avatarImage.UriSource = new System.Uri(dialog.FileName);
        avatarImage.EndInit();
        avatarImage.Freeze();

        LoadedAvatar.ImageSource = avatarImage;
        LoadedAvatarStatusText.Text = System.IO.Path.GetFileName(dialog.FileName);
    }
}
