using OpenTK.Mathematics;
using PegasusEngine.Project.Scenes;
using PegasusEngine.Renderer.Textures;

namespace PegasusEngine.Renderer;

public interface IRenderer : IDisposable
{
    /// <summary>
    /// Called by the Application once during start-up.
    /// </summary>
    public void Init();
    
    /// <summary>
    /// Called by the Application for each frame.
    /// </summary>
    public Texture2D Render(Scene scene);
    
    /// <summary>
    /// Called by the Application when the window resizes.
    /// </summary>
    public void Resize(int width, int height);
    
    /// <summary>
    /// Updates all resources.
    /// </summary>
    public void UpdateResources();

    public void Clear(Color4 color);
}