namespace NebulaControls.Controls;

public sealed class NebulaTimeClockItem
{
    public NebulaTimeClockItem(
        string label,
        int value,
        double x,
        double y,
        double centerX,
        double centerY,
        bool isSelected,
        bool isInner)
    {
        Label = label;
        Value = value;
        X = x;
        Y = y;
        CenterX = centerX;
        CenterY = centerY;
        IsSelected = isSelected;
        IsInner = isInner;
    }

    public string Label { get; }

    public int Value { get; }

    public double X { get; }

    public double Y { get; }

    public double CenterX { get; }

    public double CenterY { get; }

    public bool IsSelected { get; }

    public bool IsInner { get; }
}
