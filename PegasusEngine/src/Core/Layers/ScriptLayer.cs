using PegasusEngine.Project;
using PegasusEngine.Scripting;

namespace PegasusEngine.Core.Layers;

public sealed class ScriptLayer : Layer
{
    private ProjectManager ProjectManager { get; }
    private ScriptManager ScriptManager { get; }
    
    public ScriptLayer(ScriptManager scriptManager, ProjectManager projectManager)
    {
        this.ProjectManager = projectManager;
        this.ScriptManager = scriptManager;
    }
    
    public override void OnAttach()
    {
        if (!string.IsNullOrEmpty(ProjectManager.AbsoluteCSProjectPath))
        {
            ScriptManager.LoadScripts(ProjectManager.AbsoluteCSProjectPath, true);
        }        
    }

    public override void OnUpdate(float deltaTime)
    {
        // TODO: delta time?
        ScriptManager.UpdateScripts();
    }
}