using System.Runtime.InteropServices;
using ImGuiNET;
using PegasusEditor.Dialogs;
using PegasusEditor.ImGuiContext;
using PegasusEngine.Common;
using PegasusEngine.Core.Events;
using PegasusEngine.Objects;
using PegasusEngine.Objects.Components.Meshes;
using PegasusEngine.Project;
using PegasusEngine.Project.Scenes;

namespace PegasusEditor.TabPanels;

public class Hierarchy : TabPanel
{
    private readonly EditorState editorState;
    private readonly ProjectManager projectManager;
    
    private GUID entityPendingDeletion = GUID.INVALID;
    private GUID draggedEntity = GUID.INVALID;
    
    private bool openDeletePopup = false;

    public Hierarchy(EditorState editorState, ProjectManager projectManager)
    {
        this.editorState = editorState;
        this.projectManager = projectManager;
    }

    public override void Start()
    {
        this.Title = FontAwesomeIcons.ChartBar + " Hierarchy";
    }

    public override void Render()
    {
        var theme = editorState.Temp.EditorTheme;
        ImGui.Begin(Title);

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

        if (ImGui.BeginPopupContextWindow("HierarchyContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
        {
            if (ImGui.MenuItem("Create Empty Entity"))
            {
                var newEntity = scene.CreateEntity();
                editorState.Temp.SelectedEntity = newEntity; // Auto-select new objects
            }
            ImGui.EndPopup();
        }
        
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(3, 3));
        
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow |
                                   ImGuiTreeNodeFlags.OpenOnDoubleClick |
                                   ImGuiTreeNodeFlags.SpanAvailWidth |
                                   ImGuiTreeNodeFlags.AllowOverlap |
                                   ImGuiTreeNodeFlags.Framed |
                                   ImGuiTreeNodeFlags.FramePadding;
        
        foreach (var (guid, entity) in scene.Entities)
        {
            if (entity.Transform.Parent == null)
            {
                DrawEntityNode(entity, scene);
            }
        }

        if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && ImGui.IsWindowHovered() && !ImGui.IsAnyItemHovered())
        {
            editorState.Temp.SelectedEntity = null;
        }
        
        ImGui.PopStyleVar();
        
        HandleDeletion(scene);
        
        // Drop to root
        ImGui.Dummy(ImGui.GetContentRegionAvail());
        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload("ENTITY");
            unsafe
            {
                if (payload.NativePtr != null && draggedEntity != GUID.INVALID)
                {
                    var droppedObj = scene.Find(draggedEntity);
                    if (droppedObj != null)
                    {
                        droppedObj.Transform.SetParent(null);
                    }
                }
            }
            
            var meshPayload = ImGui.AcceptDragDropPayload(DNDPayloadTypes.Mesh);
            unsafe
            {
                if (meshPayload.NativePtr != null)
                {
                    var dndPayload = Marshal.PtrToStructure<DNDPayload>(meshPayload.Data);
                    var newEntity = scene.CreateEntity(dndPayload.Title ?? "New Mesh Object");
                    
                    var filter = newEntity.AddComponent<MeshFilter>();
                    filter.MeshGuid = new GUID(dndPayload.GuidValue);
                    newEntity.AddComponent<MeshRenderer>();
                    
                    editorState.Temp.SelectedEntity = newEntity;
                }
            }
            ImGui.EndDragDropTarget();
        }
        
        if (editorState.Temp.IsInRuntimeSimulation)
            ImGui.EndDisabled();

        ImGui.End();
    }
    
    private void DrawEntityNode(GameObject entity, Scene scene)
    {
        var theme = editorState.Temp.EditorTheme;
        
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow |
                                   ImGuiTreeNodeFlags.OpenOnDoubleClick |
                                   ImGuiTreeNodeFlags.SpanAvailWidth |
                                   ImGuiTreeNodeFlags.FramePadding |
                                   ImGuiTreeNodeFlags.AllowOverlap;
        
        if (entity.Transform.ChildCount == 0)
            flags |= ImGuiTreeNodeFlags.Leaf;

        bool isSelected = (entity == editorState.Temp.SelectedEntity);
        if (isSelected)
            flags |= ImGuiTreeNodeFlags.Selected;

        ImGui.PushID(entity.Guid.GetHashCode());

        if (isSelected)
        {
            theme.PushColor(ImGuiCol.Header, EditorCol.Secondary1);
            theme.PushColor(ImGuiCol.HeaderHovered, EditorCol.Secondary2);
            theme.PushColor(ImGuiCol.HeaderActive, EditorCol.Primary1);
        }

        bool isExpanded = ImGui.TreeNodeEx("##Node", flags, FontAwesomeIcons.Cube + " " + entity.Name);

        if (isSelected)
            theme.PopColor(3);
        
        // Drag & Drop as children
        
        if (ImGui.BeginDragDropSource())
        {
            draggedEntity = entity.Guid;
            ImGui.SetDragDropPayload("ENTITY", IntPtr.Zero, 0);
            ImGui.Text(entity.Name);
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload("ENTITY");
            unsafe
            {
                if (payload.NativePtr != null && draggedEntity != GUID.INVALID)
                {
                    var droppedObj = scene.Find(draggedEntity);
                    if (droppedObj != null && droppedObj.Guid != entity.Guid)
                    {
                        droppedObj.Transform.SetParent(entity.Transform);
                    }
                    draggedEntity = GUID.INVALID;
                }
            }
            
            var meshPayload = ImGui.AcceptDragDropPayload(DNDPayloadTypes.Mesh);
            unsafe
            {
                if (meshPayload.NativePtr != null)
                {
                    var dndPayload = Marshal.PtrToStructure<DNDPayload>(meshPayload.Data);
                    var newEntity = scene.CreateEntity(dndPayload.Title ?? "New Mesh Object");
                    
                    newEntity.Transform.SetParent(entity.Transform);

                    var filter = newEntity.AddComponent<MeshFilter>();
                    filter.MeshGuid = dndPayload.Guid;
                    
                    editorState.Temp.SelectedEntity = newEntity;
                }
            }
            
            ImGui.EndDragDropTarget();
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            editorState.Temp.SelectedEntity = entity;
        }

        if (isSelected)
        {
            float buttonPosX = ImGui.GetContentRegionAvail().X - 25.0f;
            ImGui.SameLine(buttonPosX);
            
            theme.PushColor(ImGuiCol.Button, EditorCol.Primary1, 0f); // Transparent button background
            if (ImGui.Button(FontAwesomeIcons.Trash))
            {
                entityPendingDeletion = entity.Guid;
                openDeletePopup = true;
            }
            theme.PopColor();
        }

        if (isExpanded)
        {
            foreach (var childTransform in entity.Transform.Children)
            {
                DrawEntityNode(childTransform.GameObject, scene);
            }
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private void HandleDeletion(Scene scene)
    {
        if (openDeletePopup)
        {
            ImGui.OpenPopup(FontAwesomeIcons.Trash + " Delete Entity");
            openDeletePopup = false;
        }
        
        if (entityPendingDeletion != GUID.INVALID)
        {
            var entityToDelete = scene.Find(entityPendingDeletion);
            if (entityToDelete != null)
            {
                bool triggerDelete = true;
                ConfirmationDialog.ConfirmAndExecute(
                    ref triggerDelete,
                    FontAwesomeIcons.Trash + " Delete Entity",
                    $"Are you sure you want to delete '{entityToDelete.Name}'?",
                    () =>
                    {
                        DestroyEntityTree(entityToDelete, scene);
                        
                        if (editorState.Temp.SelectedEntity?.Guid == entityPendingDeletion)
                            editorState.Temp.SelectedEntity = null;
                        
                        entityPendingDeletion = GUID.INVALID;
                    },
                    editorState
                );

                if (!triggerDelete) 
                    entityPendingDeletion = GUID.INVALID;
            }
            else
            {
                entityPendingDeletion = GUID.INVALID;
            }
        }
    }
    
    private void DestroyEntityTree(GameObject entity, Scene scene)
    {
        for (int i = entity.Transform.ChildCount - 1; i >= 0; i--)
        {
            DestroyEntityTree(entity.Transform.GetChild(i).GameObject, scene);
        }
        
        entity.Transform.SetParent(null);
        scene.RemoveEntity(entity.Guid);
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