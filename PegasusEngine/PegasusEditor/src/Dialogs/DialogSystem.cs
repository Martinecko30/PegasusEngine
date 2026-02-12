using System.Runtime.InteropServices;

namespace PegasusEngine.PegasusEditor.Dialogs;

public static partial class DialogSystem
{
    public static string FolderPickerDialog(string title = "Select Folder")
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsDialogs.OpenFolder(title);
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // return LinuxDialogs.OpenFolder(title);
        }
        
        throw new PlatformNotSupportedException("FolderPickerDialog is not supported on this platform.");
    }
    
    public static string FilePickerDialog(string filter, string title = "Select File")
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsDialogs.OpenFile(filter, title);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // return LinuxDialogs.OpenFile(filter, title);
        }

        throw new PlatformNotSupportedException("FilePickerDialog is not supported on this platform.");
    }
}