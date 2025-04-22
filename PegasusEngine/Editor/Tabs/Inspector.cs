using System.Reflection;
using ImGuiNET;
using OpenTK.Mathematics;
using PegasusEngine.Engine;

namespace PegasusEngine.Editor.Tabs;

public class Inspector : TabPanel
{
    public override void Start(EngineWindow engine)
    {
        
    }

    public override void Render()
    {
        ImGui.Begin("Inspector");
        
        if (Hierarchy.SelectedGameObject != null)
        {
            var selectedObject = Hierarchy.SelectedGameObject;
            ImGui.Text(selectedObject.Name);

            ImGui.Text("Transform");
            System.Numerics.Vector3 pos = new System.Numerics.Vector3();
            ImGui.Text("Position:");
            if (ImGui.DragFloat3("Pos", ref pos))
                selectedObject.Transform.Position = new Vector3(pos.X, pos.Y, pos.Z);
            
            System.Numerics.Vector4 rot = new System.Numerics.Vector4();
            ImGui.Text("Rotation:");
            if (ImGui.DragFloat4("Rot", ref rot))
                selectedObject.Transform.Rotation = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
            
            foreach (var behaviour in selectedObject.Behaviours)
            {
                ImGui.Text(behaviour.GetType().Name);
                // Dictionary<string,object> variableValues =
                //     behaviour.GetType().GetFields(BindingFlags.Instance | 
                //                                 BindingFlags.Static | BindingFlags.Public)
                //         .ToDictionary(f => f.Name, f => f.GetValue(behaviour));
                //
                // foreach (var variable in variableValues)
                // {
                //     Console.WriteLine(variable.Value.GetType());
                //     if (variable.Value.GetType().ToString() == "System.String")
                //     {
                //         var value = variable.Value as string;
                //         ImGui.InputText("##" + variable.Key + "##", ref value, 128);
                //     }
                // }
            }
        }
        
        ImGui.End();
    }
}