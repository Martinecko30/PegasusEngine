using System.Numerics;
using ImGuiNET;
using OpenTK.Windowing.Desktop;
using PegasusEditor.ImGuiContext;
using PegasusEditor.TabPanels;
using PegasusEngine.Core;
using PegasusEngine.Core.Events;
using PegasusEngine.Core.Layers;
using PegasusEngine.Debug;
using PegasusEngine.Project;
using PegasusEngine.Scripting;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace PegasusEditor;

public class EditorLayer : ILayer
{
    // Engine systems
    private readonly GameWindow window;
    private readonly Profiler profiler;
    private readonly IEventDispatcher eventDispatcher;
    private readonly ProjectManager projectManager;
    private readonly ScriptManager scriptManager;
    
    // Editor systems
    private readonly EditorState editorState;
    private readonly ImGuiController imguiContext;
    
    private readonly Launcher launcher;
    private readonly List<TabPanel> editorPanels = new();

    public EditorLayer(GameWindow window, Profiler profiler, IEventDispatcher eventDispatcher,
        ProjectManager projectManager, ImGuiController imguiContext, ScriptManager scriptManager)
    {
        this.window = window;
        this.profiler = profiler;
        this.eventDispatcher = eventDispatcher;
        this.projectManager = projectManager;
        this.imguiContext = imguiContext;
        this.scriptManager = scriptManager;
        
        editorState = new EditorState();
        launcher = new Launcher(editorState, projectManager);
        
        editorPanels.Add(new AssetBrowser(editorState, projectManager));
        editorPanels.Add(new Game());
        editorPanels.Add(new Hierarchy(editorState, projectManager));
        editorPanels.Add(new Inspector(editorState, projectManager));
        editorPanels.Add(new Viewport());
        editorPanels.Add(new ConsolePanel());
    }
    
    public void OnAttach()
    {
        editorState.Deserialize();

        foreach (TabPanel tabPanel in editorPanels)
            tabPanel.Start();
    }

    public void OnDetach()
    {
        editorState.Serialize();
    }

    public void OnUpdate(float deltaTime)
    {
        imguiContext.Update(window, deltaTime);
        
        editorState.Temp.EditorTheme.ApplyAllToImgui();

        float yOffset = 0f;
        if (window.IsFullscreen)
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

        if (!projectManager.ProjectIsOpen)
        {
            launcher.OnImGuiRender(hostFlags);
        }
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

            foreach (var panel in editorPanels)
            {
                panel.Update();
                panel.Render();
            }

            // TODO: Move to editor managing
            var keyboard = window.KeyboardState;
            if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyPressed(Keys.S))
            {
                projectManager.SaveProject();
            }

            if (keyboard.IsKeyPressed(Keys.F5))
            {
                if (isInRuntime)
                {
                    Log.EditorInfo("Exiting Runtime Simulation");
                    projectManager.SceneManager!.ExitRuntimeSimulation();
                    isInRuntime = false;
                }
                else
                {
                    Log.EditorInfo("Entering Runtime Simulation");
                    scriptManager.LoadScripts(projectManager.AbsoluteCSProjectPath, true);
                    projectManager.SceneManager!.EnterRuntimeSimulation();
                    isInRuntime = true;
                }
            }

            ImGui.End();
        }
        
        imguiContext.Render();
    }
    // TODO: Remove This
    private bool isInRuntime = false;
    

    public void OnEvent(IEvent e)
    {
        if (!editorState.Temp.IsInRuntimeSimulation && e.IsInputEvent)
        {
            e.Consume();
            return;
        }

        foreach (var panel in editorPanels)
        {
            panel.OnEvent(e);
            if (e.IsConsumed)
                break;
        }
        
        if (e.GetEventType() != EventType.NewFrameRendered)
            Log.EditorInfo(e.GetEventType().ToString());
    }
}