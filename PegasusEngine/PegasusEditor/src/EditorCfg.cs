namespace PegasusEngine.PegasusEditor;

public class EditorCfg
{
    public static string ExecutableDir { get; private set; } = string.Empty;
    public static string ResourcesPath { get; private set; } = string.Empty;

    // This would typically be defined in your .csproj or via build symbols
    // Equivalent to CMAKE_EDITOR_RESOURCES_PATH
    private const string DevelopmentResourcesPath = "../../../PegasusEditor/res";

    public static void Init(string? exeDir = null)
    {
        // If no path is provided, use the application's base directory
        ExecutableDir = exeDir ?? AppDomain.CurrentDomain.BaseDirectory;

#if BUILD_INSTALL
        // In an installed build, resources are usually in a subfolder
        ResourcesPath = Path.Combine(ExecutableDir, "editor_res");
#else
        // In development, we use the path defined at compile time
        // Path.GetFullPath ensures the relative dev path is resolved correctly
        ResourcesPath = Path.GetFullPath(Path.Combine(ExecutableDir, DevelopmentResourcesPath));
#endif
    }
}