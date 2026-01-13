// using System.Numerics;
// using System.Reflection;
// using ImGuiNET;
// using PegasusEngine.Modules.Scripting;
// using PegasusEngine.Utils;
// using Quaternion = OpenTK.Mathematics.Quaternion;
// using Vector2 = OpenTK.Mathematics.Vector2;
//
// namespace PegasusEngine.Editor.Tabs;
//
// public class Inspector : TabPanel
// {
//     // public override void Start(EditorWindow engine)
//     // {
//     //     Title = "Inspector";
//     // }
//
//     public override void Render()
//     {
//         ImGui.Begin(Title);
//         
//         if (Hierarchy.SelectedGameObject != null)
//         {
//             var selectedObject = Hierarchy.SelectedGameObject;
//             ImGui.Text(selectedObject.Name);
//
//             ImGui.Text("Transform");
//             Vector3 pos = MathUtils.TransVector3(selectedObject.Transform.Position);
//             ImGui.Text("Position:");
//             if (ImGui.DragFloat3("Pos", ref pos))
//                 selectedObject.Transform.Position = MathUtils.TransVector3(pos);
//             
//             var tRot = selectedObject.Transform.Rotation;
//             Vector4 rot = new Vector4(tRot.X, tRot.Y, tRot.Z, tRot.W);
//             ImGui.Text("Rotation:");
//             if (ImGui.DragFloat4("Rotation", ref rot))
//                 selectedObject.Transform.Rotation = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
//             
//             var tScal = selectedObject.Transform.Scale;
//             Vector3 scale = MathUtils.TransVector3(selectedObject.Transform.Scale);
//             ImGui.Text("Scale:");
//             if (ImGui.DragFloat3("Scale", ref scale))
//                 selectedObject.Transform.Scale = MathUtils.TransVector3(scale);
//             
//             Dictionary<string, int> idMap = new Dictionary<string, int>();
//             foreach (Behaviour behaviour in selectedObject.Behaviours)
//             {
//                 ImGui.Separator();
//                 ImGui.Text(behaviour.GetType().Name);
//                 Type type = behaviour.GetType();
//                 FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//
//                 foreach (FieldInfo field in fields)
//                 {
//                     object value = field.GetValue(behaviour);
//                     string name = field.Name;
//                     int id = idMap.GetValueOrDefault(name, 0);
//                     idMap[name] = id + 1;
//                     name = $"##{name}{id}";
//
//                     if (field.FieldType == typeof(int))
//                     {
//                         int intVal = (int)value;
//                         if (ImGui.DragInt(name, ref intVal))
//                             field.SetValue(behaviour, intVal);
//                     }
//                     else if (field.FieldType == typeof(float))
//                     {
//                         float floatVal = (float)value;
//                         if (ImGui.DragFloat(name, ref floatVal))
//                             field.SetValue(behaviour, floatVal);
//                     }
//                     else if (field.FieldType == typeof(bool))
//                     {
//                         bool boolVal = (bool)value;
//                         if (ImGui.Checkbox(name, ref boolVal))
//                             field.SetValue(behaviour, boolVal);
//                     }
//                     else if (field.FieldType == typeof(Vector2))
//                     {
//                         Vector2 vec2Val = (Vector2)value;
//                         System.Numerics.Vector2 vec2 = MathUtils.TransVector2(vec2Val);
//                         if(ImGui.DragFloat2(name, ref vec2))
//                             field.SetValue(behaviour, MathUtils.TransVector2(vec2));
//                     }
//                     else if (field.FieldType == typeof(OpenTK.Mathematics.Vector3))
//                     {
//                         OpenTK.Mathematics.Vector3 vec3Val = (OpenTK.Mathematics.Vector3)value;
//                         Vector3 vec3 = MathUtils.TransVector3(vec3Val);
//                         if(ImGui.DragFloat3(name, ref vec3))
//                             field.SetValue(behaviour, MathUtils.TransVector3(vec3));
//                     }
//                     else if (field.FieldType == typeof(String))
//                     {
//                         string strVal = (string)value;
//                         if(ImGui.InputText(name, ref strVal,UInt16.MaxValue))
//                             field.SetValue(behaviour, strVal);
//                     }
//                 }
//             }
//         }
//         
//         ImGui.End();
//     }
//
//     public override void Update()
//     {
//         
//     }
// }