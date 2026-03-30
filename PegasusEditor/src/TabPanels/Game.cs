using ImGuiNET;
using PegasusEditor.ImGuiContext;
using PegasusEngine.Core.Events;

namespace PegasusEditor.TabPanels;

public class Game : TabPanel
{
    public override void Start()
    {
        Title = FontAwesomeIcons.Gamepad + " Game";
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
        
    }

    public override void Dispose()
    {
        
    }
}