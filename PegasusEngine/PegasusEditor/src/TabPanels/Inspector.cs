using ImGuiNET;
using PegasusEngine.Pegasus.Core.Events;
using PegasusEngine.Pegasus.Project;

namespace PegasusEngine.PegasusEditor.TabPanels;

public class Inspector : TabPanel
{
    private readonly EditorState _editorState;
    private readonly ProjectManager _projectManager;
    
    public Inspector(EditorState editorState, ProjectManager projectManager)
    {
        _editorState = editorState;
        _projectManager = projectManager;
    }
    
    public override void Start()
    {
        Title = "Inspector";
    }

    public override void Render()
    {
        var theme = _editorState.Temp.EditorTheme;
        ImGui.Begin(Title);
        
        ImGui.End();
    }

    public override void Update()
    {
        
    }

    public override void OnEvent(IEvent e)
    {
        
    }
}