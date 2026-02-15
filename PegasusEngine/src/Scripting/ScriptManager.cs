using System.Diagnostics;
using System.Runtime.Loader;
using PegasusEngine.Core;

namespace PegasusEngine.Scripting;

public sealed class ScriptManager
{
    public Dictionary<string, Type> Scripts { get; private set; } = new();
    
    public void LoadScripts(string absoluteCSProjectPath, bool build = false)
    {
        if (build)
            BuildScripts(absoluteCSProjectPath);
        
        // TODO: Build scripts
        try
        {
            string dllPath = GetBinaryPath(absoluteCSProjectPath);
            
            byte[] assemblyData = File.ReadAllBytes(dllPath);
            using var stream = new MemoryStream(assemblyData);
            
            var assembly = AssemblyLoadContext.Default.LoadFromStream(stream);
            
            Scripts.Clear();
            
            var allTypes = assembly.GetTypes();
            foreach (var type in allTypes)
            {
                Log.EngineInfo($"Type: {type.Name}");
                Scripts.Add(type.Name, type);
                var script = Activator.CreateInstance(type);
                var start = type.GetMethod("Start");
                start?.Invoke(script, null);
            }
            
            Log.EngineInfo($"LoadScripts: Loaded scripts!");
        }
        catch (Exception e)
        {
            Log.EngineError($"LoadScripts: Failed to load scripts from {absoluteCSProjectPath}: {e.Message}");
        }
    }
    
    public bool BuildScripts(string absoluteCSProjectPath)
    {
        if (string.IsNullOrEmpty(absoluteCSProjectPath))
        {
            Log.EngineError("BuildScripts: CS Project path is null or empty!");
            return false;
        }
        
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{absoluteCSProjectPath}\" -c Release",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process? process = Process.Start(startInfo))
        {
            if (process == null)
                return false;
            
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            
            process.WaitForExit();

            if (process.ExitCode != 0)
            {

                Log.EditorError($"BuildScripts: Build Failed! Errors: {error}");
                return false;
            }
            
            Log.EditorInfo("BuildScripts: Build Succeeded!");
            return true;
        }
    }
    
    public static string GetBinaryPath(string absoluteCSProjectPath)
    {
        string projectDir = Path.GetDirectoryName(absoluteCSProjectPath)!;
        string projectName = Path.GetFileNameWithoutExtension(absoluteCSProjectPath);
    
        return Path.Combine(projectDir, "bin", "Release", "net9.0-windows", $"{projectName}.dll");
    }

    public void UpdateScripts(float? deltaTime = null)
    {
        // TODO: Update scripts
    }
}