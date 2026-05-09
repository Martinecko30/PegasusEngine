using System.Runtime.Loader;

namespace PegasusEngine.Scripting;

public class ScriptAssemblyLoadContext : AssemblyLoadContext
{
    public ScriptAssemblyLoadContext() : base("PegasusScriptContext", isCollectible: true)
    {
    }
}