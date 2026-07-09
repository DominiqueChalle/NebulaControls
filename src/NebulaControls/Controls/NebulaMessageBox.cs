// Nom: NebulaMessageBox
// Version: V1.00
// Description: MessageBox-style convenience facade built on top of NebulaDialog.

using System.Windows;

namespace NebulaControls.Controls;

public static class NebulaMessageBox
{
    public static NebulaMessageBoxResult Show(
        Window? owner,
        NebulaMessageBoxIcon icon,
        string title,
        string message)
    {
        return Show(owner, icon, title, message, NebulaMessageBoxButtons.OK);
    }

    public static NebulaMessageBoxResult Show(
        Window? owner,
        NebulaMessageBoxIcon icon,
        string title,
        string message,
        NebulaMessageBoxButtons buttons)
    {
        var buttonText = GetButtonText(buttons);
        var dialogResult = NebulaDialog.ShowModal(
            owner,
            title,
            message,
            ToDialogVariant(icon),
            buttonText.Primary,
            buttonText.Secondary,
            buttonText.Tertiary,
            GetWindowTitle(icon));

        return ToMessageBoxResult(buttons, dialogResult);
    }

    public static NebulaMessageBoxResult ShowInfo(Window? owner, string title, string message)
    {
        return Show(owner, NebulaMessageBoxIcon.Info, title, message);
    }

    public static NebulaMessageBoxResult ShowSuccess(Window? owner, string title, string message)
    {
        return Show(owner, NebulaMessageBoxIcon.Success, title, message);
    }

    public static NebulaMessageBoxResult ShowWarning(Window? owner, string title, string message)
    {
        return Show(owner, NebulaMessageBoxIcon.Warning, title, message);
    }

    public static NebulaMessageBoxResult ShowDanger(Window? owner, string title, string message)
    {
        return Show(owner, NebulaMessageBoxIcon.Danger, title, message);
    }

    public static NebulaMessageBoxResult ShowQuestion(Window? owner, string title, string message)
    {
        return Show(owner, NebulaMessageBoxIcon.Question, title, message, NebulaMessageBoxButtons.YesNo);
    }

    private static NebulaDialogVariant ToDialogVariant(NebulaMessageBoxIcon icon)
    {
        return icon switch
        {
            NebulaMessageBoxIcon.Success => NebulaDialogVariant.Success,
            NebulaMessageBoxIcon.Warning => NebulaDialogVariant.Warning,
            NebulaMessageBoxIcon.Danger => NebulaDialogVariant.Danger,
            NebulaMessageBoxIcon.Question => NebulaDialogVariant.Question,
            _ => NebulaDialogVariant.Info
        };
    }

    private static string GetWindowTitle(NebulaMessageBoxIcon icon)
    {
        return icon switch
        {
            NebulaMessageBoxIcon.Success => "NebulaMessageBox Success",
            NebulaMessageBoxIcon.Warning => "NebulaMessageBox Warning",
            NebulaMessageBoxIcon.Danger => "NebulaMessageBox Danger",
            NebulaMessageBoxIcon.Question => "NebulaMessageBox Question",
            _ => "NebulaMessageBox Info"
        };
    }

    private static (string Primary, string? Secondary, string? Tertiary) GetButtonText(NebulaMessageBoxButtons buttons)
    {
        return buttons switch
        {
            NebulaMessageBoxButtons.OKCancel => ("OK", "Cancel", null),
            NebulaMessageBoxButtons.YesNo => ("Yes", "No", null),
            NebulaMessageBoxButtons.YesNoCancel => ("Yes", "No", "Cancel"),
            _ => ("OK", null, null)
        };
    }

    private static NebulaMessageBoxResult ToMessageBoxResult(NebulaMessageBoxButtons buttons, NebulaDialogResult result)
    {
        if (result == NebulaDialogResult.Close)
        {
            return NebulaMessageBoxResult.Close;
        }

        return buttons switch
        {
            NebulaMessageBoxButtons.OKCancel => result switch
            {
                NebulaDialogResult.Primary => NebulaMessageBoxResult.OK,
                NebulaDialogResult.Secondary => NebulaMessageBoxResult.Cancel,
                _ => NebulaMessageBoxResult.None
            },
            NebulaMessageBoxButtons.YesNo => result switch
            {
                NebulaDialogResult.Primary => NebulaMessageBoxResult.Yes,
                NebulaDialogResult.Secondary => NebulaMessageBoxResult.No,
                _ => NebulaMessageBoxResult.None
            },
            NebulaMessageBoxButtons.YesNoCancel => result switch
            {
                NebulaDialogResult.Primary => NebulaMessageBoxResult.Yes,
                NebulaDialogResult.Secondary => NebulaMessageBoxResult.No,
                NebulaDialogResult.Tertiary => NebulaMessageBoxResult.Cancel,
                _ => NebulaMessageBoxResult.None
            },
            _ => result == NebulaDialogResult.Primary
                ? NebulaMessageBoxResult.OK
                : NebulaMessageBoxResult.None
        };
    }
}
