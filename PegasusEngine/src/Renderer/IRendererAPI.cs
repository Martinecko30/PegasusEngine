using OpenTK.Mathematics;

namespace PegasusEngine.Renderer;

public interface IRendererAPI
{
    public void Init();
    public void Clear(Color4 color);
    public void SetViewportSize(int width, int height);
}