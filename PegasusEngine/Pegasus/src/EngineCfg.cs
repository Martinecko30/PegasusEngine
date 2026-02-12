namespace PegasusEngine.Pegasus;

public class EngineCfg
{
    // path to the exe currently running the engine
    public static string ExecutableDir { get; private set; } = string.Empty;
    public static string ResourcesPath { get; private set; } = string.Empty;

    public static void Init(string exeDir)
    {
        ExecutableDir = exeDir;

#if DEBUG
        // Replace this with the relative or absolute path to your 
        // source-controlled resources during development
        ResourcesPath = Path.GetFullPath(Path.Combine(exeDir, "../../../Resources"));
#else
        // In a build/install scenario, resources are usually 
        // in a folder next to the executable
        ResourcesPath = Path.Combine(exeDir, "engine_res");
#endif
    }
}