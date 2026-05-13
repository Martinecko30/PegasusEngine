using System.Globalization;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PegasusEngine.Debug;

namespace PegasusEditor;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        
        EditorCfg.Init();

        var settings = new NativeWindowSettings()
        {
            Title = "Pegasus Engine",
            Size = new OpenTK.Mathematics.Vector2i(1280, 720),
            Flags = ContextFlags.Debug
        };

        var editor = new EditorApplication(GameWindowSettings.Default, settings);
        Log.EditorInfo("Editor started.");

        editor.Run();
    }
}