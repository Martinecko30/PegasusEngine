namespace PegasusEditor;

public class EditorCfg
{
    public static string ExecutableDir { get; private set; } = string.Empty;
    public static string ResourcesPath { get; private set; } = string.Empty;

    public static void Init(string? exeDir = null)
    {
        // If no path is provided, use the application's base directory
        ExecutableDir = exeDir ?? AppDomain.CurrentDomain.BaseDirectory;

#if BUILD_INSTALL
        // In an installed build, resources are usually in a subfolder
        ResourcesPath = Path.Combine(ExecutableDir, "editor_res");
#else
        DirectoryInfo? currentDir = new DirectoryInfo(ExecutableDir);

        while (currentDir != null)
        {
            string potentialResPath = Path.Combine(currentDir.FullName, "res");
            if (Directory.Exists(potentialResPath))
            {
                ResourcesPath = potentialResPath;
                return;
            }
            
            currentDir = currentDir.Parent;
        }
        
        throw new DirectoryNotFoundException("Could not find 'res' folder in the executable directory.");
#endif
    }
}