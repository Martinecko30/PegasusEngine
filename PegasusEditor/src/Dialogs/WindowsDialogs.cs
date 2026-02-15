using System.Windows.Forms;

namespace PegasusEditor.Dialogs;

public class WindowsDialogs
{
    public static string OpenFolder(string title)
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = title;
        dialog.UseDescriptionForTitle = true;

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            return dialog.SelectedPath;
        }
        return string.Empty;
    }

    public static string OpenFile(string filter, string title)
    {
        using var dialog = new OpenFileDialog();
        dialog.Title = title;
        // Convert C++ style extension (e.g. ".pgproj") to WinForms format ("Project Files (*.pgproj)|*.pgproj")
        dialog.Filter = $"Files (*{filter})|*{filter}|All files (*.*)|*.*";

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            return dialog.FileName;
        }
        return string.Empty;
    }
}