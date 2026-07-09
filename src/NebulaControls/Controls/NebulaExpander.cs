// Nom: NebulaExpander
// Version: V1.03
// Description: Expander base control styled for Nebula expandable sections.

using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaExpander : Expander
{
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Handled || !IsEnabled)
        {
            return;
        }

        if (e.Key is Key.Add or Key.OemPlus)
        {
            IsExpanded = true;
            e.Handled = true;
        }
        else if (e.Key is Key.Subtract or Key.OemMinus)
        {
            IsExpanded = false;
            e.Handled = true;
        }
    }
}
