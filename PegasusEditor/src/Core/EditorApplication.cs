using System.Runtime.InteropServices;
using OpenTK.Windowing.Desktop;
using PegasusEngine.Core;
using PegasusEditor.ImGuiContext;
using PegasusEditor.Renderer;
using PegasusEngine.Debug;
using PegasusEngine.Renderer;
using OpenTK.Graphics.OpenGL;

namespace PegasusEditor;

public class EditorApplication : Application
{
    private ImGuiController _ImGuiController;
    private static DebugProc DebugMessageDelegate = OnDebugMessage;
    
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

    protected override IRenderer CreateRenderer()
    {
        var renderer = new EditorRenderer(ProjectManager.AssetManager);
        
        // Setup debug callback
        GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);

        return renderer;
    }

    public new void Shutdown()
    {
        _ImGuiController.Dispose();
        base.Shutdown();
    }
    
    private static void OnDebugMessage(
        DebugSource source,     // Source of the debugging message.
        DebugType type,         // Type of the debugging message.
        int id,                 // ID associated with the message.
        DebugSeverity severity, // Severity of the message.
        int length,             // Length of the string in pMessage.
        IntPtr pMessage,        // Pointer to message string.
        IntPtr pUserParam)      // The pointer you gave to OpenGL, explained later.
    {
        string message = Marshal.PtrToStringAnsi(pMessage, length);

        string formattedMessage = string.Format("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message); 
        if (type == DebugType.DebugTypeError)
            Log.EditorError(formattedMessage);
        else if (type == DebugType.DebugTypeDeprecatedBehavior || type == DebugType.DebugTypePerformance)
            Log.EditorWarn(formattedMessage);
        else
            Log.EditorDebug(formattedMessage);
        
        // if (type == DebugType.DebugTypeError)
        // {
        //     throw new Exception(message);
        // }
    }
}