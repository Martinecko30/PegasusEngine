using OpenTK.Windowing.Desktop;
using PegasusEngine.Core;
using PegasusEditor.ImGuiContext;

namespace PegasusEditor;

public class EditorApplication : Application
{
    private ImGuiController _ImGuiController;
    
    public EditorApplication(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        _ImGuiController = new ImGuiController(nativeWindowSettings.ClientSize.X, nativeWindowSettings.ClientSize.Y);
        
        Window.Resize += e => _ImGuiController.WindowResized(e.Width, e.Height);
        
        Window.TextInput += e =>
        {
            if (e.Unicode != 0)
                _ImGuiController.PressChar((char)e.Unicode);
        };
        
        LayerStack.PushLayer(new EditorLayer(
            Window,
            Profiler,
            LayerStack,
            ProjectManager,
            _ImGuiController,
            ScriptManager
            ));
    }

    public new void Shutdown()
    {
        _ImGuiController.Dispose();
        base.Shutdown();
    }
}