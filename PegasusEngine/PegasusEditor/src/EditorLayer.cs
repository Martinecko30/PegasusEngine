using System.Numerics;
using ImGuiNET;
using OpenTK.Windowing.Desktop;
using PegasusEngine.Pegasus.Core;
using PegasusEngine.Pegasus.Core.Events;
using PegasusEngine.Pegasus.Core.Layers;
using PegasusEngine.Pegasus.Project;
using PegasusEngine.PegasusEditor.ImGuiContext;
using PegasusEngine.PegasusEditor.TabPanels;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace PegasusEngine.PegasusEditor;

public class EditorLayer : ILayer
{
    // Engine systems
    private GameWindow _window;
    private Profiler _profiler;
    private IEventDispatcher _eventDispatcher;
    private ProjectManager _projectManager;
    
    // Editor systems
    private EditorState _editorState;
    private ImGuiController _imguiContext;
    
    private Launcher _launcher;
    private List<TabPanel> _editorPanels = new();

    public EditorLayer(GameWindow window, Profiler profiler, IEventDispatcher eventDispatcher,
        ProjectManager projectManager, ImGuiController imguiContext)
    {
        _window = window;
        _profiler = profiler;
        _eventDispatcher = eventDispatcher;
        _projectManager = projectManager;
        _imguiContext = imguiContext;
        
        _editorState = new EditorState();
        _launcher = new Launcher(_editorState, projectManager);
        
        _editorPanels.Add(new AssetBrowser(_editorState, projectManager));
        _editorPanels.Add(new Game());
        _editorPanels.Add(new Hierarchy());
        _editorPanels.Add(new Inspector(_editorState, projectManager));
        _editorPanels.Add(new Viewport());
    }
    
    public void OnAttach()
    {
        _editorState.Deserialize();

        foreach (TabPanel tabPanel in _editorPanels)
            tabPanel.Start();
    }

    public void OnDetach()
    {
        _editorState.Serialize();
    }

    public void OnUpdate(float deltaTime)
    {
        _imguiContext.Update(_window, deltaTime);
        
        _editorState.Temp.EditorTheme.ApplyAllToImgui();

        float yOffset = 0f;
        if (_window.IsFullscreen)
            yOffset = 6f;
        
        // _windowTitleBar.OnImGuiRender(yOffset);
        
        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.Pos with { Y = viewport.Pos.Y + yOffset }); // TODO: Titlebar offset
        ImGui.SetNextWindowSize(viewport.Size with { Y = viewport.Size.Y - yOffset }); // TODO: minus titlebar height
        ImGui.SetNextWindowViewport(viewport.ID);
        ImGuiWindowFlags hostFlags = ImGuiWindowFlags.NoDocking |
                                     ImGuiWindowFlags.NoTitleBar |
                                     ImGuiWindowFlags.NoCollapse |
                                     ImGuiWindowFlags.NoResize |
                                     ImGuiWindowFlags.NoMove |
                                     ImGuiWindowFlags.NoBringToFrontOnFocus |
                                     ImGuiWindowFlags.NoNavFocus;

        if (!_projectManager.ProjectIsOpen)
            _launcher.OnImGuiRender(hostFlags);
        else
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0f, 0f));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.Begin("##DockSpaceHost", hostFlags);
            ImGui.PopStyleVar(2);
            var style = ImGui.GetStyle();
            float minWinSizeX = style.WindowMinSize.X;
            style.WindowMinSize.X = 300f;
            ImGui.DockSpace(ImGui.GetID("MyDockspace"));
            style.WindowMinSize.X = minWinSizeX;

            foreach (var panel in _editorPanels)
                panel.Render();

            var keys = _window.KeyboardState;
            if (keys.IsKeyDown(Keys.LeftControl) && keys.IsKeyDown(Keys.S))
            {
                _projectManager.SaveProject();
            }

            ImGui.End();
        }
        
        _imguiContext.Render();
    }

    public void OnEvent(IEvent e)
    {
        if (!_editorState.Temp.IsInRuntimeSimulation && e.IsInputEvent)
        {
            e.Consume();
            return;
        }

        foreach (var panel in _editorPanels)
        {
            panel.OnEvent(e);
            if (e.IsConsumed)
                break;
        }
        
        if (e.GetEventType() != EventType.NewFrameRendered)
            Log.EditorInfo(e.GetEventType().ToString());
    }
}