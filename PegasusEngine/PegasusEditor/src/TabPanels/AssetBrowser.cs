using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using FontAwesome;
using ImGuiNET;
using PegasusEngine.Pegasus.Core;
using PegasusEngine.Pegasus.Core.Events;
using PegasusEngine.Pegasus.Project;
using PegasusEngine.Pegasus.Project.Assets;
using PegasusEngine.PegasusEditor.Dialogs;
using PegasusEngine.PegasusEditor.ImGuiContext;

namespace PegasusEngine.PegasusEditor.TabPanels;

public class AssetBrowser : TabPanel
{
    private EditorState _editorState;
    private ProjectManager _projectManager;

    private const float
        BASE_TILE_WH_RATIO = 0.75f,
        BASE_TILE_ICON_FONT_SIZE = 0.2f,
        BASE_TILE_TITLE_FONT_SIZE = 16.0f,
        TITLE_SCALAR_MIN = 75.0f,
        TITLE_SCALAR_MAX = 110.0f;
    
    private float _TileScalar = 110.0f;

    private GUID _selectedTileGuid = GUID.INVALID;
    
    private DNDPayload _dndPayload;
    
    private bool _shouldDeleteScene = false, _shouldDeleteAsset = false;
    

    public AssetBrowser(EditorState editorState, ProjectManager projectManager)
    {
        _editorState = editorState;
        _projectManager = projectManager;
    }
    
    public override void Start()
    {
        Title = "Assets";
    }

    public override void Render()
    {
        var theme = _editorState.Temp.EditorTheme;
        
        theme.PushColor(ImGuiCol.WindowBg, EditorCol.Background1);
        ImGui.Begin(FontAwesomeIcons.Cubes + " Assets");
        if (_editorState.Temp.IsInRuntimeSimulation)
            ImGui.BeginDisabled();

        if (!_projectManager.ProjectIsOpen)
        {
            if (_editorState.Temp.IsInRuntimeSimulation)
                ImGui.EndDisabled();
            ImGui.End();
            theme.PopColor();
            return;
        }

        var assetManager = _projectManager.AssetManager;
        var assetPool = assetManager?.AssetPool;
        
        theme.PushColor(ImGuiCol.Button, EditorCol.Secondary2);
        
        if (ImGui.Button("Add"))
        {
            ImGui.OpenPopup("Add Menu");
        }

        if (ImGui.IsPopupOpen("Add Menu"))
        {
            var addButtonPos = ImGui.GetItemRectMin();
            var addButtonSize = ImGui.GetItemRectSize();
            ImGui.SetNextWindowSizeConstraints(
                new Vector2(float.MinValue, float.MinValue),
                new Vector2(float.MaxValue, 300.0f)
                );
            ImGui.SetNextWindowPos(addButtonPos with { Y = addButtonPos.Y + addButtonSize.Y });
            ImGui.SetNextWindowSize(new Vector2(0f, 0f));
        }

        if (ImGui.BeginPopup("Add Menu", ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (ImGui.MenuItem(FontAwesomeIcons.CircleNodes + " Scene"))
                _projectManager.SceneManager?.CreateScene();

            if (ImGui.MenuItem(FontAwesomeIcons.Cube + " Asset"))
            {
                var assetPath = WindowsDialogs.OpenFile("*.*", "Select Asset:");
                if (!string.IsNullOrEmpty(assetPath))
                    assetManager!.ImportAsset(assetPath);
            }
            ImGui.EndPopup();
        }
        theme.PopColor();
        
        ImGui.SameLine();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 5.0f);
        ImGui.SliderFloat("##TitleSize", ref _TileScalar, TITLE_SCALAR_MIN, TITLE_SCALAR_MAX, "");

        if (ImGui.BeginTable("##AssetTable", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.NoSavedSettings))
        {
            ImGui.TableNextColumn();
            ImGui.BeginChild("LeftPanel", new Vector2(ImGui.GetContentRegionAvail().Y), ImGuiChildFlags.None);
            {
                unsafe
                {
                    if (ImGui.IsMouseClicked(0) &&
                        ImGui.IsWindowHovered() &&
                        !ImGui.IsAnyItemHovered() &&
                        ImGui.GetDragDropPayload().NativePtr == null)
                    {
                        _selectedTileGuid = GUID.INVALID;
                    }
                }

                float horizontalSpacing = 15.0f;
                float verticalSpacing = 3.0f;
                var style = ImGui.GetStyle();
                float column_x1 = ImGui.GetCursorPos().X;
                float column_x2 = column_x1 + ImGui.GetColumnWidth();
                ImGui.Indent(horizontalSpacing);
                foreach (var (guid, scene) in _projectManager.SceneManager!)
                {
                    DrawSceneTile(guid, scene.Name);
                    float lastAssetTile_x2 = ImGui.GetItemRectMax().X;
                    if (lastAssetTile_x2 + ImGui.GetItemRectSize().X < column_x2)
                        ImGui.SameLine(0, horizontalSpacing);
                    else
                        ImGui.Dummy(new Vector2(0, verticalSpacing));
                }

                foreach (var (guid, pair) in assetPool!.Metadata)
                {
                    var (metadata, extension) = pair;
                    string filename = Path.GetFileName(extension.SourcePath);
                    DrawAssetTile(guid, filename);
                    float lastAssetTile_x2 = ImGui.GetItemRectMax().X;
                    if (lastAssetTile_x2 + ImGui.GetItemRectSize().X < column_x2)
                        ImGui.SameLine(0, horizontalSpacing);
                    else
                        ImGui.Dummy(new Vector2(0, verticalSpacing));
                }
                ImGui.Unindent(horizontalSpacing);
            }
            ImGui.EndChild();

            ImGui.TableNextColumn();
            DrawTileInfo();
            ImGui.EndTable();
        }
        
        if (_editorState.Temp.IsInRuntimeSimulation)
            ImGui.EndDisabled();
        
        ImGui.End();
        theme.PopColor();
    }

    public override void Update()
    {
        
    }

    public override void OnEvent(IEvent e)
    {
        
    }

    private void DrawSceneTile(GUID guid, string title)
    {
        DrawGenericTile(guid, title, FontAwesomeIcons.CircleNodes, DNDPayloadTypes.Scene);
    }

    private void DrawAssetTile(GUID guid, string title)
    {
        if (!_projectManager.ProjectIsOpen)
            return;

        var assetPool = _projectManager.AssetManager.AssetPool;
        string icon = string.Empty;
        string dndPayloadType = string.Empty;

        if (assetPool.FindMetadata<MeshMetadata>(guid) != null)
        {
            icon = FontAwesomeIcons.Cube;
            dndPayloadType = DNDPayloadTypes.Mesh;
        }
        else if (assetPool.FindMetadata<TextureMetadata>(guid) != null)
        {
            icon = FontAwesomeIcons.FileImage;
            dndPayloadType = DNDPayloadTypes.Texture;
        } else
            return; // Skip unsupported asset types

        DrawGenericTile(guid, title, icon, dndPayloadType);
    }

    private void DrawGenericTile(GUID guid, string title, string icon, string dndPayloadType)
    {
        ImGui.PushID(guid.ToString());
        var theme = _editorState.Temp.EditorTheme;
        var drawList = ImGui.GetWindowDrawList();
        
        var tileBg = _selectedTileGuid == guid ? EditorCol.Secondary1 : EditorCol.Primary3;
        theme.PushColor(ImGuiCol.Header, tileBg);
        
        if (ImGui.Selectable(
                "##tile",
                true,
                ImGuiSelectableFlags.None,
                new Vector2(
                    _TileScalar * BASE_TILE_WH_RATIO,
                    _TileScalar)
                ))
        {
            _selectedTileGuid = guid;
        }
        theme.PopColor();

        if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceAllowNullID))
        {
            _dndPayload.GuidValue = (ulong)guid;
            _dndPayload.Title = title;

            unsafe
            {
                int size = Marshal.SizeOf<DNDPayload>();
                byte* buffer = stackalloc byte[size];
                IntPtr ptr = (IntPtr)buffer;

                Marshal.StructureToPtr(_dndPayload, ptr, fDeleteOld: false);
                ImGui.SetDragDropPayload(dndPayloadType, ptr, (uint)size);
            }
            
            theme.PushColor(ImGuiCol.Text, EditorCol.Text2);
            ImGui.Text($"{icon} {title}");
            theme.PopColor();
            
            ImGui.EndDragDropSource();
        }

        var tileTopLeft = ImGui.GetItemRectMin();
        var tileBottomRight = ImGui.GetItemRectMax();
        var tileDims = ImGui.GetItemRectSize();
        
        if (_selectedTileGuid != guid)
            drawList.PushClipRect(tileTopLeft, tileBottomRight, true);

        // Highlight active scene tile
        var tileFgCol = EditorCol.Text2;
        var sceneManager = _projectManager.SceneManager;
        var openScene = sceneManager?.GetOpenScene();
        if (openScene != null && openScene.Guid == guid)
            tileFgCol = EditorCol.Text1;

        var font = ImGui.GetFont();
        float iconFontSize = MathF.Floor(BASE_TILE_ICON_FONT_SIZE * _TileScalar);
        var iconTextSize = ImGui.CalcTextSize(icon);
        var iconPos = new Vector2(
                tileTopLeft.X + (tileDims.X - iconTextSize.X) * 0.5f,
                tileTopLeft.Y + (tileDims.X - iconTextSize.X) * 0.5f
            );
        drawList.AddText(
            font,
            iconFontSize,
            iconPos,
            ImGui.GetColorU32(theme[tileFgCol]),
            icon
        );

        float margin = 5.0f;
        var titlePos = new Vector2(
                MathF.Floor(tileTopLeft.X + margin),
                MathF.Floor(tileTopLeft.Y + tileDims.Y * BASE_TILE_WH_RATIO)
            );
        float wrapWidth = tileDims.X - margin * 2.0f;
        
        drawList.AddText(
            ImGui.GetFont(),
            BASE_TILE_TITLE_FONT_SIZE,
            titlePos,
            ImGui.GetColorU32(theme[tileFgCol]),
            title,
            wrap_width: wrapWidth
        );
        
        if (_selectedTileGuid != guid)
            drawList.PopClipRect();
        
        ImGui.PopID();
    }

    private void DrawTileInfo()
    {
        if (!_projectManager.ProjectIsOpen)
            return;


        var assetPool = _projectManager.AssetManager.AssetPool;
        var sceneManger = _projectManager.SceneManager;

        ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X);

        var theme = _editorState.Temp.EditorTheme;
        void DrawLabelValue(string label, string value)
        {
            theme.PushColor(ImGuiCol.Text, EditorCol.Text2);
            ImGui.TextUnformatted(label);
            theme.PopColor();
            ImGui.SameLine();
            ImGui.TextUnformatted(value);
        }

        if (_selectedTileGuid == GUID.INVALID)
        {
            ImGui.PopTextWrapPos();
            return;
        }

        var scene = sceneManger?.Find(_selectedTileGuid);
        if (scene != null)
        {
            Debug.Assert(scene.Guid == _selectedTileGuid);

            theme.PushColor(ImGuiCol.Text, EditorCol.Accent1);
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("General");
            theme.PopColor();

            ImGui.SameLine();

            bool currentSceneOpen = scene.Guid == sceneManger!.GetOpenSceneGuid();
            string scnBtnLabel = currentSceneOpen ? "Close" : "Open";
            string delScnBtnLabel = FontAwesomeIcons.Trash;

            var scnLabelSize = ImGui.CalcTextSize(scnBtnLabel);
            var delScnLabelSize = ImGui.CalcTextSize(delScnBtnLabel);

            var scnBtnSize = new Vector2(
                scnLabelSize.X + ImGui.GetStyle().FramePadding.X * 2.0f,
                scnLabelSize.Y + ImGui.GetStyle().FramePadding.Y * 2.0f
            );
            var delScnBtnSize = new Vector2(
                delScnLabelSize.X + ImGui.GetStyle().FramePadding.X * 2.0f,
                delScnLabelSize.Y + ImGui.GetStyle().FramePadding.Y * 2.0f
            );

            float btnGroupWidth = scnBtnSize.X + ImGui.GetStyle().ItemSpacing.X + delScnBtnSize.X;

            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - btnGroupWidth);

            if (ImGui.Button(scnBtnLabel, scnBtnSize))
                sceneManger.SetOpenSceneGuid(currentSceneOpen ? GUID.INVALID : scene.Guid);

            ImGui.SameLine();

            if (ImGui.Button(delScnBtnLabel + "##DeleteSceneBtn", delScnBtnSize))
                _shouldDeleteScene = true;


            ConfirmationDialog.ConfirmAndExecute(
                ref _shouldDeleteScene,
                FontAwesomeIcons.Trash + " Delete Scene",
                $"Are you sure you want to delete '{scene.Name}'?",
                onConfirm: () =>
                {
                    sceneManger.DeleteScene(scene.Guid);
                    _selectedTileGuid = GUID.INVALID;
                },
                _editorState
            );
            
            theme.PushColor(ImGuiCol.Text, EditorCol.Text2);
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("Scene Name:");
            theme.PopColor();
            
            ImGui.SameLine();
            
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            string name = scene.Name;
            if (ImGui.InputTextWithHint("##sceneNameInput", "Scene Name", ref name, 256))
                scene.Name = name;
            
            float clearSkyboxBtnWidth = ImGui.CalcTextSize(FontAwesomeIcons.Trash).X + ImGui.GetStyle().FramePadding.X * 2.0f + ImGui.GetStyle().ItemSpacing.X;
            string skyboxDisplayName = string.IsNullOrEmpty(scene.SkyboxName) ? "No skybox selected" : scene.SkyboxName;
            
            float totalWidgetWidth = ImGui.GetContentRegionAvail().X - clearSkyboxBtnWidth;

            ImGuiDndWidgets.DragDropWidget(
                "Skybox:",
                DNDPayloadTypes.Texture,
                skyboxDisplayName,
                payload =>
                {
                    scene.SkyboxName = payload.Title;
                    scene.SkyboxGuid = payload.Guid;
                },
                theme,
                "Drag a texture asset here to set as skybox",
                new(totalWidgetWidth, 0.0f),
                !string.IsNullOrEmpty(scene.SkyboxName)
            );
            
            ImGui.SameLine();
            if (ImGui.Button(FontAwesomeIcons.Trash + "##Clear Skybox"))
            {
                scene.SkyboxName = string.Empty;
                scene.SkyboxGuid = GUID.INVALID;
            }
            
            theme.PushColor(ImGuiCol.Text, EditorCol.Text2);
            ImGui.TextUnformatted("Open on Boot:");
            theme.PopColor();
            
            ImGui.SameLine();
            bool isBoot = _projectManager.IsBootScene(scene.Guid);
            
            theme.PushColor(ImGuiCol.CheckMark, EditorCol.Text1);
            if (ImGui.Checkbox("##bootSceneCheckbox", ref isBoot))
                _projectManager.BootSceneGuid = isBoot ? scene.Guid : GUID.INVALID;
            theme.PopColor();
            
            ImGui.Dummy(new Vector2(0.0f, 8.0f));
            
            theme.PushColor(ImGuiCol.Text, EditorCol.Accent1);
            ImGui.TextUnformatted("Scene Info");
            theme.PopColor();
            
            DrawLabelValue("GUID:", scene.Guid.ToString());
        } else if (assetPool.Metadata.TryGetValue(_selectedTileGuid, out var pair))
        {
            theme.PushColor(ImGuiCol.Text, EditorCol.Accent1);
            ImGui.TextUnformatted("General");
            theme.PopColor();
            ImGui.AlignTextToFramePadding();
            ImGui.SameLine();
            const string deleteLabel = FontAwesomeIcons.Trash;
            var textSize = ImGui.CalcTextSize(deleteLabel);
            var buttonSize = new Vector2(
                textSize.X + ImGui.GetStyle().FramePadding.X * 2.0f,
                textSize.Y + ImGui.GetStyle().FramePadding.Y * 2.0f
            );
            
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - buttonSize.X);
            if (ImGui.Button(deleteLabel))
                _shouldDeleteAsset = true;
            
            ConfirmationDialog.ConfirmAndExecute(
                ref _shouldDeleteAsset,
                deleteLabel + " Delete Asset",
                "Are you sure you want to delete this asset?",
                () =>
                {
                    // TODO: delete asset
                    _selectedTileGuid = GUID.INVALID;
                },
                _editorState
            );
            
            var (metadata, extension) = pair;
            DrawLabelValue("Source Path:", extension.SourcePath);
            DrawLabelValue("File Size:", ConvertFileSize(extension.FileSizeInBytes));
            DrawLabelValue("Load Time:", $"{extension.LoadTimeMs:0.00} ms");
            DrawLabelValue("Guid:", _selectedTileGuid.ToString());
            
            ImGui.Dummy(new Vector2(0.0f, 5.0f));

            if (metadata is MeshMetadata mesh)
            {
                theme.PushColor(ImGuiCol.Text, EditorCol.Accent1);
                ImGui.TextUnformatted("Mesh Metadata");
                theme.PopColor();

                DrawLabelValue("Triangle Count:", mesh.TriCount.ToString());
                DrawLabelValue("FirstTriIdx:", mesh.FirstTriIdx.ToString());
                ImGui.Dummy(new Vector2(0, 3.0f));
                DrawLabelValue("BVH Node Count:", mesh.NodeCount.ToString());
                DrawLabelValue("BVH FirstNodeIdx:", mesh.FirstNodeIdx.ToString());
            }
            else if (metadata is TextureMetadata tex)
            {
                theme.PushColor(ImGuiCol.Text, EditorCol.Accent1);
                ImGui.TextUnformatted("Texture Metadata");
                theme.PopColor();

                DrawLabelValue("Width:", tex.Width.ToString());
                DrawLabelValue("Height:", tex.Height.ToString());
                DrawLabelValue("Channels:", tex.Channels.ToString());
            }
            else
            {
                theme.PushColor(ImGuiCol.Text, EditorCol.Error);
                ImGui.Text("[ERROR] Invalid Asset guid");
                theme.PopColor();
            }
        }

        ImGui.PopTextWrapPos();
    }
    
    private static string ConvertFileSize(ulong bytes)
    {
        const double KB = 1024.0;
        const double MB = 1024.0 * KB;
        const double GB = 1024.0 * MB;

        if (bytes >= (ulong)GB) return $"{bytes / GB:0.00} GB";
        if (bytes >= (ulong)MB) return $"{bytes / MB:0.00} MB";
        if (bytes >= (ulong)KB) return $"{bytes / KB:0.00} KB";
        return $"{bytes} Bytes";
    }
}