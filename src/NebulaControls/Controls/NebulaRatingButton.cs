using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NebulaControls.Controls;

public class NebulaRatingButton : Button
{
    public static readonly DependencyProperty RatingItemProperty =
        DependencyProperty.Register(
            nameof(RatingItem),
            typeof(NebulaRatingItem),
            typeof(NebulaRatingButton),
            new PropertyMetadata(null));

    public NebulaRatingItem? RatingItem
    {
        get => (NebulaRatingItem?)GetValue(RatingItemProperty);
        set => SetValue(RatingItemProperty, value);
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        FindAncestorRating()?.PreviewRatingItem(RatingItem);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        FindAncestorRating()?.ClearRatingPreview();
    }

    protected override void OnClick()
    {
        base.OnClick();
        FindAncestorRating()?.SetRatingItem(RatingItem);
    }

    private NebulaRating? FindAncestorRating()
    {
        DependencyObject? current = this;

        while (current is not null)
        {
            if (current is NebulaRating rating)
            {
                return rating;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
