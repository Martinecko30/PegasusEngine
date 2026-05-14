namespace PegasusEditor;

/// <summary>
/// Provides editor-specific configuration paths used to locate the executable directory and editor resources.
/// </summary>
public class EditorCfg
{
    /// <summary>
    /// Gets the directory containing the editor executable.
    /// </summary>
    public static string ExecutableDir { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the path to the editor resources directory.
    /// </summary>
    public static string ResourcesPath { get; private set; } = string.Empty;

    /// <summary>
    /// Initializes the editor configuration paths.
    /// </summary>
    /// <param name="exeDir">
    /// The executable directory to use. If <see langword="null"/>, the application's base directory is used.
    /// </param>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when the resources directory cannot be found in a non-installed build.
    /// </exception>
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