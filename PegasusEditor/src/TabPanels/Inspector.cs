using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTK.Mathematics;
using PegasusEditor.ImGuiContext;
using PegasusEngine.Common;
using PegasusEngine.Core.Events;
using PegasusEngine.Debug;
using PegasusEngine.Objects;
using PegasusEngine.Objects.Components;
using PegasusEngine.Objects.Components.Colliders;
using PegasusEngine.Objects.Components.Lights;
using PegasusEngine.Objects.Components.Meshes;
using PegasusEngine.Project;
using PegasusEngine.Project.Scenes.Serialization;

namespace PegasusEditor.TabPanels;

public class Inspector : TabPanel
{
    private readonly EditorState editorState;
    private readonly ProjectManager projectManager;
    
    private readonly Dictionary<Type, Action<object, FieldInfo, string>> typeHandlers;

    private List<Type> availableComponentTypes;
    
    public Inspector(EditorState editorState, ProjectManager projectManager)
    {
        this.editorState = editorState;
        this.projectManager = projectManager;

        typeHandlers = new()
        {
            {typeof(bool), RenderBoolField},
            {typeof(string), RenderStringField},
            {typeof(Vector2), RenderVector2Field},
            {typeof(Vector3), RenderVector3Field},
            {typeof(Vector4), RenderVector4Field},
            {typeof(int), RenderIntField},
            {typeof(float), RenderFloatField},
            {typeof(Quaternion), RenderQuaternionField},
            {typeof(GUID), RenderGUIDField}
        };
    }
    
    public override void Start()
    {
        Title = FontAwesomeIcons.Sliders + " Inspector";
    }

    public override void Render()
    {
        var theme = editorState.Temp.EditorTheme;
        
        ImGui.SetNextWindowSizeConstraints(new(350, 50), new(float.MaxValue, float.MaxValue));
        ImGui.Begin(Title);

        if (editorState.Temp.IsInRuntimeSimulation)
            ImGui.BeginDisabled();
        
        var scene = projectManager.SceneManager?.GetOpenScene();

        if (scene == null || editorState.Temp.SelectedEntity == null)
        {
            if (editorState.Temp.IsInRuntimeSimulation)
                ImGui.EndDisabled();
            
            ImGui.End();
            return;
        }
        
        theme.PushColor(ImGuiCol.FrameBg, EditorCol.Primary2);
        var selected = editorState.Temp.SelectedEntity;
        
        if (selected != null)
            RenderEntity(selected);
        else
            ImGui.Text("Select an object to inspect.");

        
        if (editorState.Temp.IsInRuntimeSimulation)
            ImGui.EndDisabled();
        
        ImGui.End();
    }

    private void RenderEntity(GameObject entity)
    {
        var theme = editorState.Temp.EditorTheme;
        
        // Render name
        theme.PushColor(ImGuiCol.Text, EditorCol.Text2);
        ImGui.AlignTextToFramePadding();
        ImGui.Text(FontAwesomeIcons.Tag + " Name:");
        ImGui.SameLine();
        theme.PopColor();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            
        string output = string.Empty;
        if (ImGui.InputTextWithHint("##Name", entity.Name, ref output, 256))
            entity.Name = output;
            
        // Render GUID
        theme.PushColor(ImGuiCol.Text, EditorCol.Text2);
        ImGui.AlignTextToFramePadding();
        ImGui.Text(FontAwesomeIcons.Hashtag + " Guid:");
        ImGui.SameLine();
        theme.PopColor();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.Text(entity.Guid.ToString());
            
        foreach (var component in entity.Components)
            RenderComponent(component);
        
        ImGui.Dummy(new(0f, 10f));
        var panelDims = ImGui.GetContentRegionAvail();
        float lineHeight = ImGui.GetFont().FontSize + ImGui.GetStyle().FramePadding.Y * 2f;
        ImGui.SetCursorPosX(panelDims.X / 6);
        bool popupOpened = false;
        float borderSz = ImGui.GetStyle().PopupBorderSize;
        ImGui.GetStyle().PopupBorderSize = 0f;
        theme.PushColor(ImGuiCol.Button, EditorCol.Secondary2);
        float buttonWidth = panelDims.X * (2f / 3f);

        if (ImGui.Button("Add Component", new(buttonWidth, lineHeight)))
        {
            popupOpened = true;
            ImGui.OpenPopup("AddComponent");
        }
        theme.PopColor();

        if (ImGui.IsPopupOpen("AddComponent"))
        {
            var addButtonPos = ImGui.GetItemRectMin();
            var addButtonSize = ImGui.GetItemRectSize();
            ImGui.SetNextWindowSizeConstraints(
                new(float.MinValue, float.MinValue),
                new(float.MaxValue, 0f)
            );
            ImGui.SetNextWindowPos(addButtonSize with { Y = addButtonPos.Y + addButtonSize.Y });
            ImGui.SetNextWindowSize(new(buttonWidth, 0f));
        }

        if (ImGui.BeginPopup("AddComponent"))
        {
            // TODO: Add entity components
            GiveEntityComponentButton<BoxCollider>(entity, "Box Collider", FontAwesomeIcons.Box);
            GiveEntityComponentButton<Camera>(entity, "Camera", FontAwesomeIcons.Camera);
            GiveEntityComponentButton<MeshFilter>(entity, "Mesh Filter", FontAwesomeIcons.Filter);
            GiveEntityComponentButton<MeshRenderer>(entity, "Mesh Renderer", FontAwesomeIcons.Bucket);
            GiveEntityComponentButton<Light>(entity, "Light", FontAwesomeIcons.Lightbulb);
            
            ImGui.BulletText($"Add Component to entity {entity.Tag}");
            ImGui.EndPopup();
        }
        ImGui.GetStyle().PopupBorderSize = borderSz;
        
        ImGui.Dummy(new(0f, 100f));
        theme.PopColor();
    }

    private void RenderComponent(Component component)
    {
        var type = component.GetType();

        if (ImGui.CollapsingHeader(type.Name, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.PushID(component.GetHashCode());

            var fields = GetInspectorFields(type);

            foreach (var field in fields)
            {
                if (typeHandlers.TryGetValue(field.FieldType, out var handler))
                    handler(component, field, field.Name);
                else if (field.FieldType.IsEnum)
                    RenderEnumField(component, field, field.Name);
                else
                    ImGui.TextColored(new System.Numerics.Vector4(1, 1, 0, 1), $"Unsupported Type: {field.FieldType.Name} ({field.Name})");
            }
            
            ImGui.PopID();
        }
    }

    public override void Update()
    {
        
    }

    public override void OnEvent(IEvent e)
    {
        if (e is ReloadScriptAssembliesEvent reloadEvent)
        {
            availableComponentTypes.AddRange(reloadEvent.newScriptTypes);
        }
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
    }

    private IEnumerable<FieldInfo> GetInspectorFields(Type type)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        for (var cur = type; cur != null; cur = cur.BaseType)
        {
            foreach (var f in cur.GetFields(flags))
            {
                if (f.IsStatic || f.IsLiteral || f.IsInitOnly) continue;
                if (Attribute.IsDefined(f, typeof(NonSerializedAttribute))) continue;

                if (!f.IsPublic && !Attribute.IsDefined(f, typeof(SerializeFieldAttribute))) continue;

                yield return f;
            }
        }
    }



    private void GiveEntityComponentButton<T>(GameObject entity, string label, string icon) where T : Component, new()
    {
        if (entity.HasComponent<T>())
            ImGui.BeginDisabled();
        
        if (ImGui.Selectable(icon + " " + label, false))
        {
            entity.GetOrAddComponent<T>();
            return;
        }
        
        if (entity.HasComponent<T>())
            ImGui.EndDisabled();
    }
    
    #region Type Handlers
    private void RenderBoolField(object instance, FieldInfo field, string name)
    {
        bool val = (bool)(field.GetValue(instance) ?? false);
        if (ImGui.Checkbox(name, ref val))
        {
            field.SetValue(instance, val);
        }
    }
    
    private void RenderStringField(object instance, FieldInfo field, string name)
    {
        string val = (string)(field.GetValue(instance) ?? string.Empty);
        if (ImGui.InputText(name, ref val, 256))
            field.SetValue(instance, val);
    }

    // TODO: Possible mismatch between System.Numerics.Vector2 and OpenTK.Mathematics.Vector2
    private void RenderVector2Field(object instance, FieldInfo field, string name)
    {
        var val = (Vector2)(field.GetValue(instance) ?? Vector2.Zero);
        var sysVec = new System.Numerics.Vector2(val.X, val.Y);
        if (ImGui.DragFloat2(name, ref sysVec))
            field.SetValue(instance, new Vector2(sysVec.X, sysVec.Y));
    }
    
    private void RenderVector3Field(object instance, FieldInfo field, string name)
    {
        var val = (Vector3)(field.GetValue(instance) ?? Vector3.Zero);
        var sysVec = new System.Numerics.Vector3(val.X, val.Y, val.Z);
        if (ImGui.DragFloat3(name, ref sysVec))
            field.SetValue(instance, new Vector3(sysVec.X, sysVec.Y, sysVec.Z));
    }
    
    private void RenderVector4Field(object instance, FieldInfo field, string name)
    {
        var val = (Vector4)(field.GetValue(instance) ?? Vector4.Zero);
        var sysVec = new System.Numerics.Vector4(val.X, val.Y, val.Z, val.W);
        if (ImGui.DragFloat4(name, ref sysVec))
            field.SetValue(instance, new Vector4(sysVec.X, sysVec.Y, sysVec.Z, sysVec.W));
    }

    private void RenderQuaternionField(object instance, FieldInfo field, string name)
    {
        var val = (Quaternion)(field.GetValue(instance) ?? Quaternion.Identity);
        var sysQuat = new System.Numerics.Vector4(val.X, val.Y, val.Z, val.W);
        if (ImGui.DragFloat4(name, ref sysQuat))
            field.SetValue(instance, new Quaternion(sysQuat.X, sysQuat.Y, sysQuat.Z, sysQuat.W));
    }

    private void RenderIntField(object instance, FieldInfo field, string name)
    {
        int val = (int)(field.GetValue(instance) ?? 0);
        if (ImGui.DragInt(name, ref val))
        {
            field.SetValue(instance, val);
        }
    }

    private void RenderFloatField(object instance, FieldInfo field, string name)
    {
        float val = (float)(field.GetValue(instance) ?? 0f);
        if (ImGui.DragFloat(name, ref val))
        {
            field.SetValue(instance, val);
        }
    }
    
    private void RenderEnumField(object instance, FieldInfo field, string name)
    {
        var enumValues = Enum.GetValues(field.FieldType);
        var enumNames = Enum.GetNames(field.FieldType);
        
        object? currentValue = field.GetValue(instance);
        int currentIndex = currentValue != null ? Array.IndexOf(enumNames, currentValue.ToString()) : 0;
        
        if (ImGui.Combo(name, ref currentIndex, enumNames, enumNames.Length))
        {
            field.SetValue(instance, enumValues.GetValue(currentIndex));
        }
    }

    private unsafe void RenderGUIDField(object instance, FieldInfo field, string name)
    {
        GUID currentGuid = (GUID)(field.GetValue(instance)!);
    
        string displayValue = currentGuid == GUID.INVALID ? "None" : currentGuid.ToString();

        ImGui.InputText(name, ref displayValue, 256, ImGuiInputTextFlags.ReadOnly);

        if (ImGui.BeginDragDropTarget())
        {
            ImGuiPayloadPtr payload = default;

            if (payload.NativePtr == null) payload = ImGui.AcceptDragDropPayload(DNDPayloadTypes.Mesh);
            if (payload.NativePtr == null) payload = ImGui.AcceptDragDropPayload(DNDPayloadTypes.Texture);
            if (payload.NativePtr == null) payload = ImGui.AcceptDragDropPayload(DNDPayloadTypes.Scene);

            if (payload.NativePtr != null)
            {
                int expectedSize = Marshal.SizeOf<DNDPayload>();
                if (payload.DataSize == expectedSize)
                {
                    // 5. Extract the struct and assign the GUID!
                    DNDPayload droppedData = Marshal.PtrToStructure<DNDPayload>(payload.Data);
                    field.SetValue(instance, droppedData.Guid);
                }
                else
                {
                    Log.EngineError($"Drag/Drop Payload size mismatch! Expected {expectedSize}, got {payload.DataSize}");
                }
            }
    
            ImGui.EndDragDropTarget();
        }
    }
    #endregion
}