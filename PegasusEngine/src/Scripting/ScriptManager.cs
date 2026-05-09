using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using PegasusEngine.Core;
using PegasusEngine.Debug;
using PegasusEngine.Objects.Components;

namespace PegasusEngine.Scripting;

public sealed class ScriptManager
{
    private ScriptAssemblyLoadContext? scriptContext;
    public Dictionary<string, Type> ScriptTypes { get; private set; } = new();
    public List<object> ActiveScripts { get; private set; } = new();
    
    public void LoadScripts(string absoluteCSProjectPath, bool build = false)
    {
        if (build && !BuildScripts(absoluteCSProjectPath))
            return;
        
        // TODO: Build scripts
        try
        {
            if (scriptContext != null)
            {
                scriptContext.Unload();
                scriptContext = null;
                ScriptTypes.Clear();
                ActiveScripts.Clear();
                
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            
            scriptContext = new ScriptAssemblyLoadContext();
            string dllPath = GetBinaryPath(absoluteCSProjectPath);
            
            using var stream = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
            var assembly = scriptContext.LoadFromStream(stream);
            
            var allTypes = assembly.GetTypes();
            foreach (var type in allTypes)
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Component)))
                {
                    ScriptTypes.Add(type.Name, type);
                    Log.EngineInfo($"Loaded User Script: {type.Name}");
                    
                    var scriptInstance = Activator.CreateInstance(type);
                    if (scriptInstance != null)
                    {
                        ActiveScripts.Add(scriptInstance);
                        
                        var startMethod = type.GetMethod("Start", BindingFlags.Public | BindingFlags.Instance);
                        startMethod?.Invoke(scriptInstance, null);
                    }
                }
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
        foreach (var script in ActiveScripts)
        {
            Type type = script.GetType();
            var updateMethod = type.GetMethod("Update", BindingFlags.Instance);
            
            updateMethod?.Invoke(script, new object[] { deltaTime });
        }
    }
}