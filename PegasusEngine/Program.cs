#region

using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PegasusEngine.Editor;
using PegasusEngine.Pegasus.Core;

#endregion

namespace PegasusEngine;

class Program
{
    static void Main(string[] args)
    {
        var nativeWindowSettings = new NativeWindowSettings
        {
            Size = new Vector2i(800, 600),
            WindowState = WindowState.Maximized,
            Title = "PegasusEngine"
        };
        
        // var engine = new EditorWindow(GameWindowSettings.Default, nativeWindowSettings, args.ToList());
        // engine.Run();
        
        Log.Init();
        Log.EngineInfo("C# version: {0}", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
    }
}