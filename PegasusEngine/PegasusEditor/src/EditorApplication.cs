using OpenTK.Windowing.Desktop;
using PegasusEngine.PegasusEditor.ImGuiContext;
using Application = PegasusEngine.Pegasus.Core.Application;

namespace PegasusEngine.PegasusEditor;

public class EditorApplication : Application
{
    private ImGuiController _ImGuiController;
    
    public EditorApplication(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        _ImGuiController = new ImGuiController(nativeWindowSettings.ClientSize.X, nativeWindowSettings.ClientSize.Y);
        
        _Window.Resize += e => _ImGuiController.WindowResized(e.Width, e.Height);
        
        _Window.TextInput += e =>
        {
            if (e.Unicode != 0)
                _ImGuiController.PressChar((char)e.Unicode);
        };
        
        _LayerStack.PushLayer(new EditorLayer(
            _Window,
            _Profiler,
            _LayerStack,
            _ProjectManager,
            _ImGuiController
            ));
    }

    public new void Shutdown()
    {
        _ImGuiController.Dispose();
        base.Shutdown();
    }
}