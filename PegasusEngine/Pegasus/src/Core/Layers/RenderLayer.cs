using PegasusEngine.Pegasus.Project;

namespace PegasusEngine.Pegasus.Core.Layers;

public class RenderLayer : Layer
{
    private readonly Profiler _profiler;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ProjectManager _projectManager;
    // private readonly Renderer _renderer;

    public RenderLayer(IEventDispatcher eventDispatcher,
        Profiler profiler,
        ProjectManager projectManager)
    {
        
    }
}