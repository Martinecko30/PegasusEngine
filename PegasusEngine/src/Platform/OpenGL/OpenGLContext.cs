using OpenTK.Windowing.GraphicsLibraryFramework;
using PegasusEngine.Renderer;

namespace PegasusEngine.Platform.OpenGL;

public unsafe class OpenGLContext : IRenderingContext
{
    private readonly Window* _nativeWindow;

    public static void SetWindowHints()
    {
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 6);
        GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
    }
    
    public OpenGLContext(Window* nativeWindow)
    {
        _nativeWindow = nativeWindow;
    }
    
    public void Init()
    {
        GLFW.MakeContextCurrent(_nativeWindow);
    }

    public void SwapBuffers()
    {
        throw new NotImplementedException();
    }
}