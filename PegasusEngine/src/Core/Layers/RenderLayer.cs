using PegasusEngine.Core.Events;
using PegasusEngine.Debug;
using PegasusEngine.Project;
using PegasusEngine.Renderer;
using PegasusEngine.Renderer.Textures;

namespace PegasusEngine.Core.Layers;

public class RenderLayer : Layer
{
    private readonly Profiler profiler;
    private readonly IEventDispatcher eventDispatcher;
    private readonly ProjectManager projectManager;
    private readonly IRenderer renderer;

    public RenderLayer(IEventDispatcher eventDispatcher,
        Profiler profiler,
        ProjectManager projectManager,
        IRenderer renderer)
    {
        this.eventDispatcher = eventDispatcher;
        this.profiler = profiler;
        this.projectManager = projectManager;
        this.renderer = renderer;
    }

    public override void OnAttach()
    {
        renderer.Init();
        
        Log.EngineInfo("RenderLayer: Attached and Renderer Initialized.");
    }
    
    public override void OnUpdate(float deltaTime)
    {
        renderer.UpdateResources();

        var activeScene = projectManager.SceneManager?.GetOpenScene();

        if (activeScene != null)
        {
            // The renderer draws everything to its internal FBO and returns the final image
            Texture2D frame = renderer.Render(activeScene);
            
            eventDispatcher.DispatchEvent(new NewFrameRenderedEvent(frame));
        }
    }

    public override void OnDetach()
    {
        renderer.Dispose();
        Log.EngineInfo("RenderLayer: Detached and Renderer Disposed.");
    }
    
    public override void OnEvent(IEvent e)
    {
        if (e is WindowResizeEvent resizeEvent)
        {
            renderer.Resize(resizeEvent.Width, resizeEvent.Height);
        }
    }
}