using PegasusEngine.Core.Events;
using PegasusEngine.Project;
using PegasusEngine.Scripting;

namespace PegasusEngine.Core.Layers;

public sealed class ScriptLayer : Layer
{
    private ProjectManager ProjectManager { get; }
    private ScriptManager ScriptManager { get; }
    private IEventDispatcher eventDispatcher { get; }
    
    public ScriptLayer(ScriptManager scriptManager, ProjectManager projectManager, IEventDispatcher eventDispatcher)
    {
        this.ProjectManager = projectManager;
        this.ScriptManager = scriptManager;
        this.eventDispatcher = eventDispatcher;
    }
    
    public override void OnAttach()
    {
        if (!string.IsNullOrEmpty(ProjectManager.AbsoluteCSProjectPath))
        {
            ScriptManager.LoadScripts(ProjectManager.AbsoluteCSProjectPath, true);
            
            eventDispatcher.DispatchEvent(new ReloadScriptAssembliesEvent(ScriptManager.ScriptTypes.Values.ToList()));
        }        
    }

    public override void OnUpdate(float deltaTime)
    {
        ScriptManager.UpdateScripts(deltaTime);
    }
}