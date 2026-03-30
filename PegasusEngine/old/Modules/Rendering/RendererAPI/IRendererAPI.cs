namespace PegasusEngine.old.Modules.Rendering.RendererAPI;

public interface IRendererAPI
{
    void CreateBuffer();
    void CreateShader();
    void CreateTexture();
    void SetViewport(int width, int height);
    void DrawIndexed();
    void Clear();
    void Present();
}