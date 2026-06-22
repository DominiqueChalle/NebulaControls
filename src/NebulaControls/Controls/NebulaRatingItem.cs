namespace NebulaControls.Controls;

public sealed class NebulaRatingItem
{
    public NebulaRatingItem(int value, bool isSelected, bool isPreviewed)
    {
        Value = value;
        IsSelected = isSelected;
        IsPreviewed = isPreviewed;
    }

    public int Value { get; }

    public bool IsSelected { get; }

    public bool IsPreviewed { get; }
}
