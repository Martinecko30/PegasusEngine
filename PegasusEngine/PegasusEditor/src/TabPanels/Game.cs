using ImGuiNET;
using PegasusEngine.Pegasus.Core.Events;

namespace PegasusEngine.PegasusEditor.TabPanels;

public class Game : TabPanel
{
    public override void Start()
    {
        Title = "Game";
    }

    public override void Render()
    {
        ImGui.Begin(Title);
        
        ImGui.End();
    }

    public override void Update()
    {
        
    }

    public override void OnEvent(IEvent e)
    {
        throw new NotImplementedException();
    }
}