using ImGuiNET;
using PegasusEngine.Pegasus.Core.Events;
using PegasusEngine.Runtime.Objects;

namespace PegasusEngine.PegasusEditor.TabPanels;

public class Hierarchy : TabPanel
{
    
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

    public override void OnEvent(IEvent e)
    {
        throw new NotImplementedException();
    }

    // private void DrawObjectHierarchy(GameObject obj)
    // {
    //     if (ImGui.TreeNode(obj.Name))
    //     {
    //         if (ImGui.IsItemClicked())
    //             SelectedGameObject = obj;
    //         
    //         if (obj.Children.Count > 0)
    //             foreach (var child in obj.Children)
    //                 DrawObjectHierarchy(child);
    //         ImGui.TreePop();
    //     }
    // }
}