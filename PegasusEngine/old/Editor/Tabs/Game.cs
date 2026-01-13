using ImGuiNET;

namespace PegasusEngine.Editor.Tabs;

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
}