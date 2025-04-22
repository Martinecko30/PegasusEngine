using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PegasusEngine.Engine;

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
        
        var engine = new EngineWindow(GameWindowSettings.Default, nativeWindowSettings, args.ToList());
        engine.Run();
    }
}