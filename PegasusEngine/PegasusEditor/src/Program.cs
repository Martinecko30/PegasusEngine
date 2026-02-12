using OpenTK.Windowing.Desktop;
using PegasusEngine.Pegasus.Core;
using PegasusEngine.PegasusEditor;

namespace PegasusEditor;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        EditorCfg.Init();

        var settings = new NativeWindowSettings();
        settings.Title = "Pegasus Engine";
        settings.Size = new OpenTK.Mathematics.Vector2i(1280, 720);

        var editor = new EditorApplication(GameWindowSettings.Default, settings);
        Log.EditorInfo("Editor started.");

        editor.Run();
    }
}