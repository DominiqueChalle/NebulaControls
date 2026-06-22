using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NebulaControls.Controls;

public class NebulaAvatar : Control
{
    static NebulaAvatar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaAvatar),
            new FrameworkPropertyMetadata(typeof(NebulaAvatar)));
    }

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(NebulaAvatar),
            new PropertyMetadata(null));

    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public static readonly DependencyProperty AvatarKindProperty =
        DependencyProperty.Register(
            nameof(AvatarKind),
            typeof(NebulaAvatarKind),
            typeof(NebulaAvatar),
            new PropertyMetadata(NebulaAvatarKind.User));

    public NebulaAvatarKind AvatarKind
    {
        get => (NebulaAvatarKind)GetValue(AvatarKindProperty);
        set => SetValue(AvatarKindProperty, value);
    }
}
