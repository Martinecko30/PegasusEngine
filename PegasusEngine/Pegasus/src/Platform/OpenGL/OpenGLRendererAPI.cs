using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PegasusEngine.Pegasus.Core;
using PegasusEngine.Pegasus.Renderer;

namespace PegasusEngine.Pegasus.Platform.OpenGL;

public class OpenGLRendererAPI : IRendererAPI
{
    public void Init()
    {
        Log.EngineInfo("Using OpenGL Renderer API.");
    }

    public void Clear(Color4 color)
    {
        GL.ClearColor(color.R, color.G, color.B, color.A);
        GL.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void SetViewportSize(int width, int height)
    {
        GL.Viewport(0, 0, width, height);
    }
}