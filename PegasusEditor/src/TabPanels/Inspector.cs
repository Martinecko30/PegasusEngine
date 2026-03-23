using System.Numerics;
using System.Reflection;
using ImGuiNET;
using PegasusEditor.ImGuiContext;
using PegasusEngine.Core.Events;
using PegasusEngine.Objects;
using PegasusEngine.Objects.Components;
using PegasusEngine.Objects.Components.Colliders;
using PegasusEngine.Project;

namespace PegasusEditor.TabPanels;

public class Inspector : TabPanel
{
    private readonly EditorState editorState;
    private readonly ProjectManager projectManager;
    
    private readonly Dictionary<Type, Action<object, FieldInfo, string>> typeHandlers;
    
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
            {typeof(int), RenderIntField},
            {typeof(float), RenderFloatField},
        };
    }
    
    public override void Start()
    {
        Title = "Inspector";
    }

    public override void Render()
    {
        var theme = editorState.Temp.EditorTheme;
        
        ImGui.SetNextWindowSizeConstraints(new(350, 50), new(float.MaxValue, float.MaxValue));
        ImGui.Begin(FontAwesomeIcons.Sliders + Title);

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
            
            ImGui.BulletText($"Add Component to entity {entity.Tag}");
            ImGui.EndPopup();
        }
        ImGui.GetStyle().PopupBorderSize = borderSz;
        
        ImGui.Dummy(new(0f, 100f));
        theme.PopColor();
    }

    private void RenderComponent(object target)
    {
        var type = target.GetType();
        var fields = type.GetFields(
            BindingFlags.Instance |
            BindingFlags.Public
            );
        
        ImGui.TextDisabled($"{type.Name} Properties");
        ImGui.Separator();

        foreach (var field in fields)
        {
            if (typeHandlers.TryGetValue(field.FieldType, out var handler))
                handler(target, field, field.Name);
            else
                ImGui.TextColored(new Vector4(1, 1, 0, 1), $"Unknown Type: {field.FieldType.Name} ({field.Name})");
        }
    }

    public override void Update()
    {
        
    }

    public override void OnEvent(IEvent e)
    {
        
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
        string val = (string)(field.GetValue(instance) ?? false);
        if (ImGui.InputText(name, ref val, 256))
        {
            field.SetValue(instance, val);
        }
    }

    // TODO: Possible mismatch between System.Numerics.Vector2 and OpenTK.Mathematics.Vector2
    private void RenderVector2Field(object instance, FieldInfo field, string name)
    {
        Vector2 val = (Vector2)(field.GetValue(instance) ?? Vector2.Zero);
        if (ImGui.DragFloat2(name, ref val))
        {
            field.SetValue(instance, val);
        }
    }
    
    private void RenderVector3Field(object instance, FieldInfo field, string name)
    {
        Vector3 val = (Vector3)(field.GetValue(instance) ?? Vector2.Zero);
        if (ImGui.DragFloat3(name, ref val))
        {
            field.SetValue(instance, val);
        }
    }

    private void RenderIntField(object instance, FieldInfo field, string name)
    {
        int val = (int)(field.GetValue(instance) ?? 0);
        if (ImGui.InputInt(name, ref val))
        {
            field.SetValue(instance, val);
        }
    }

    private void RenderFloatField(object instance, FieldInfo field, string name)
    {
        float val = (float)(field.GetValue(instance) ?? 0f);
        if (ImGui.InputFloat(name, ref val))
        {
            field.SetValue(instance, val);
        }
    }
    #endregion
}