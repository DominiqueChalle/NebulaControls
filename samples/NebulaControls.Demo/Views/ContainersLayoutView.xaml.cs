using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NebulaControls.Controls;

namespace NebulaControls.Demo.Views;

public partial class ContainersLayoutView : UserControl
{
    public ContainersLayoutView()
    {
        InitializeComponent();
    }

    private void AdvancedOptionsExpander_Expanded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ExpanderStatusText is not null)
        {
            ExpanderStatusText.Text = "Advanced options are open";
        }
    }

    private void AdvancedOptionsExpander_Collapsed(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ExpanderStatusText is not null)
        {
            ExpanderStatusText.Text = "Advanced options are closed";
        }
    }

    private void ApplyAdvancedOptionsButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        ExpanderStatusText.Text = "Advanced options applied";
    }

    private void GroupBoxSetting_Changed(object sender, System.Windows.RoutedEventArgs e)
    {
        if (GroupBoxStatusText is not null)
        {
            GroupBoxStatusText.Text = "Settings changed";
        }
    }

    private void ApplyGroupBoxSettingsButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var featureState = GroupBoxFeatureToggle.IsChecked == true ? "enabled" : "disabled";
        var notificationState = GroupBoxNotificationCheckBox.IsChecked == true ? "notifications on" : "notifications off";
        GroupBoxStatusText.Text = $"Settings applied: feature {featureState}, {notificationState}";
    }

    private void OpenNebulaWindowButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var preview = new NebulaWindowPreview
        {
            Owner = System.Windows.Window.GetWindow(this)
        };

        preview.Show();
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

    private void TopTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TopTabStatusText is null || e.Source is not NebulaTabControl tabControl)
        {
            return;
        }

        TopTabStatusText.Text = $"Selected tab: {GetSelectedTabHeader(tabControl)}";
    }

    private static string GetSelectedTabHeader(TabControl tabControl)
    {
        return tabControl.SelectedItem is TabItem item
            ? item.Header?.ToString() ?? "Tab"
            : "None";
    }
}
