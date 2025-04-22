using ImGuiNET;
using PegasusEngine.Engine;
using PegasusEngine.Engine.Objects;

namespace PegasusEngine.Editor.Tabs;

public class Hierarchy : TabPanel
{
    public static GameObject? SelectedGameObject;
    
    public override void Start(EngineWindow engine)
    {
        
    }

    public override void Render()
    {
        ImGui.Begin("Hierarchy");
        
        var objects = EngineWindow.CurrentScene.GetObjects();
        foreach (var gameObject in objects)
        {
            DrawObjectHierarchy(gameObject);
        }
        
        ImGui.End();
    }

    private void DrawObjectHierarchy(GameObject obj)
    {
        if (ImGui.TreeNode(obj.Name))
        {
            if (ImGui.IsItemClicked())
                SelectedGameObject = obj;
            
            if (obj.Children.Count > 0)
                foreach (var child in obj.Children)
                    DrawObjectHierarchy(child);
            ImGui.TreePop();
        }
    }
}