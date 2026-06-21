namespace NebulaControls.Controls;

public sealed class NebulaTimePickerItem
{
    public NebulaTimePickerItem(string label, int value, bool isSelected)
    {
        Label = label;
        Value = value;
        IsSelected = isSelected;
    }

    public string Label { get; }

    public int Value { get; }

    public bool IsSelected { get; }
}
