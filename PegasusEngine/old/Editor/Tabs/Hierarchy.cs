using ImGuiNET;
using PegasusEngine.Runtime.Objects;

namespace PegasusEngine.Editor.Tabs;

public class Hierarchy : TabPanel
{
    public static GameObject? SelectedGameObject;
    
    public override void Start()
    {
        
    }

    public override void Render()
    {
        ImGui.Begin("Hierarchy");
        
        // var objects = EditorWindow.CurrentScene.GetObjects();
        // foreach (var gameObject in objects)
        // {
        //     DrawObjectHierarchy(gameObject);
        // }
        
        ImGui.End();
    }

    public override void Update()
    {
        
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