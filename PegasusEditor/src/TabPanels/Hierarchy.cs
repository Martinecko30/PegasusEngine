using ImGuiNET;
using PegasusEditor.Dialogs;
using PegasusEditor.ImGuiContext;
using PegasusEngine.Core;
using PegasusEngine.Core.Events;
using PegasusEngine.Objects;
using PegasusEngine.Project;
using PegasusEngine.Project.Scenes;
using PegasusEngine.Runtime.Objects;
using PegasusEngine.Scripting;

namespace PegasusEditor.TabPanels;

public class Hierarchy : TabPanel
{
    private readonly EditorState editorState;
    private readonly ProjectManager projectManager;
    
    private bool entityDestroy = false;

    public Hierarchy(EditorState editorState, ProjectManager projectManager)
    {
        this.editorState = editorState;
        this.projectManager = projectManager;
    }

    public override void Start()
    {
        this.Title = "Hierarchy";
    }

    public override void Render()
    {
        var theme = editorState.Temp.EditorTheme;
        ImGui.Begin(FontAwesomeIcons.ChartBar + Title);

        if (editorState.Temp.IsInRuntimeSimulation)
            ImGui.BeginDisabled();

        if (!projectManager.ProjectIsOpen)
        {
            if (editorState.Temp.IsInRuntimeSimulation)
                ImGui.EndDisabled();
            ImGui.End();
            return;
        }
        
        var scene = projectManager.SceneManager?.GetOpenScene();
        if (scene == null)
        {
            ImGui.Text("Scene not open!");
            ImGui.End();
            return;
        }

        if (ImGui.BeginMenu("Add"))
        {
            if (ImGui.MenuItem("Empty"))
                scene.CreateEntity();

            ImGui.EndMenu();
        }
        
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(3, 3));
        
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow |
                                   ImGuiTreeNodeFlags.OpenOnDoubleClick |
                                   ImGuiTreeNodeFlags.SpanAvailWidth |
                                   ImGuiTreeNodeFlags.AllowOverlap |
                                   ImGuiTreeNodeFlags.Framed |
                                   ImGuiTreeNodeFlags.FramePadding;
        
        foreach (var (guid, entity) in scene.Entities)
            DrawObject(guid, entity, scene);

        if (ImGui.IsMouseDown(0) && ImGui.IsWindowHovered())
            editorState.Temp.SelectedEntity = null;
        
        ImGui.PopStyleVar();
        
        if (editorState.Temp.IsInRuntimeSimulation)
            ImGui.EndDisabled();

        ImGui.End();
    }

    private void DrawObject(GUID guid, GameObject entity, Scene scene)
    {
        var theme = editorState.Temp.EditorTheme;
        var panelDims = ImGui.GetContentRegionAvail();
        float lineHeight = ImGui.GetFont().FontSize + ImGui.GetStyle().FramePadding.Y * 2f;
        bool entityChildrenOpen = false;
            
        theme.PushColor(ImGuiCol.Text, EditorCol.Text2);

        if (entity == editorState.Temp.SelectedEntity)
        {
            theme.PushColor(ImGuiCol.Text, EditorCol.Text1);
            theme.PushColor(ImGuiCol.Header, EditorCol.Secondary1);
            theme.PushColor(ImGuiCol.Button, EditorCol.Primary1, 0f);
                
            entityChildrenOpen = ImGui.TreeNode(entity.Name);
            ImGui.SameLine(panelDims.X - lineHeight * 0.5f);
            if (ImGui.Button(FontAwesomeIcons.Trash))
                entityDestroy = true;
            ConfirmationDialog.ConfirmAndExecute(
                ref entityDestroy,
                FontAwesomeIcons.Trash + " Delete Entity",
                "Are you sure you want to delete " + entity.Name + "?",
                () =>
                {
                    scene.RemoveEntity(guid);
                    editorState.Temp.SelectedEntity = null;
                },
                editorState
            );
                
            theme.PopColor(3);
        }
        else
        {
            entityChildrenOpen = ImGui.TreeNode(entity.Name);
        }
        theme.PopColor();

        if (ImGui.IsItemClicked())
        {
            editorState.Temp.SelectedEntity = entity;
        }

        if (entityChildrenOpen)
        {
            foreach (var child in entity.Transform.Children)
                DrawObject(child.Guid, child.GameObject, scene);
            ImGui.TreePop();
        }
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