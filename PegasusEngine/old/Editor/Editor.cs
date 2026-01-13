using PegasusEngine.Editor.ImGUI;
using PegasusEngine.Editor.Tabs;
using PegasusEngine.Editor.Tools;
using PegasusEngine.Editor.Undo;
using PegasusEngine.Runtime.Objects;
using PegasusEngine.Runtime.Scenes;

namespace PegasusEngine.Editor;

public class Editor
{
    public static Editor Instance;
    
    private readonly ImGuiController controller;

    private List<TabPanel> panels = new List<TabPanel>();
    public Scene ActiveScene { get; private set; }
    
    public IEditorTool ActiveTool;
    public UndoSystem Undo;
    

    public void Initialize()
    {
        if (Instance != null)
            throw new Exception("Editor already initialized");

        Instance = this;
        
        // // Reigster Panels
        // RegisterPanel(new Viewport());
        // RegisterPanel(new AssetBrowser());
        // RegisterPanel(new Game());
        // RegisterPanel(new Inspector());
        // RegisterPanel(new Hierarchy());
    }

    public void Update()
    {
        foreach (var panel in panels)
            panel.Update();
    }

    public void Render()
    {
        
    }

    public void Shutdown()
    {
        
    }

    public void RegisterPanel(TabPanel panel)
    {
        panels.Add(panel);
    }

    public void DrawAllPanels()
    {
        foreach (var panel in panels)
            panel.Render();
    }

    public void RenderSceneView()
    {
        
    }

    public void HandleGizmos()
    {
        
    }

    public void HandleSceneSelection()
    {
        
    }

    public void CreateGameObject(GameObject obj)
    {
        ActiveScene.AddObject(obj);
    }

    public void DeleteGameObject(GameObject obj)
    {
        
    }

    public void DuplicateGameObject(GameObject obj)
    {
        ActiveScene.AddObject(obj);
    }

    public void EnterPlayMode()
    {
        
    }

    public void ExitPlayMode()
    {
        
    }

    public void Pause()
    {
        
    }

    public void Step()
    {
        
    }

    public void SetActiveTool(IEditorTool tool)
    {
        
    }


    #region OldCode
    /*
    private readonly ImGuiController controller;
    
    private readonly List<TabPanel> tabPanels = new List<TabPanel>();


    readonly EditorRenderer editorRenderer = new EditorRenderer();
    public static bool DEBUG { private set; get; }
    public static bool PLAYING { private set; get; }
    
    //public static Scene CurrentScene { private set; get; }
    private Stopwatch timer;

    // Fix: Declare `CurrentScene` properly and initialize correctly during editor runtime.
    // TODO: This
    public static Scene? CurrentScene { private set; get; }

    public EditorWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, List<string> args) : base(gameWindowSettings, nativeWindowSettings)
    {
        // Process arguments
        DEBUG = args.Contains("--debug");
        PLAYING = false; // Fix: Initialize PLAYING to false at start to avoid unintentional behavior.

        // Proper initialization order
        editorRenderer.Start(ClientSize, Size);
        controller = editorRenderer.GetImGuiController();

        // Add OpenGL settings for smooth rendering and anti-aliasing
        GLFW.WindowHint(WindowHintInt.Samples, 4);

        CursorState = CursorState.Normal; // Ensure the cursor starts in Normal mode.
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
        List<string> skyboxFaces = new List<string>
        {
            "Resources\\Skybox\\right.jpg",
            "Resources\\Skybox\\left.jpg",
            "Resources\\Skybox\\top.jpg",
            "Resources\\Skybox\\bottom.jpg",
            "Resources\\Skybox\\front.jpg",
            "Resources\\Skybox\\back.jpg"
        };

        // Fix: Initialize or load the `CurrentScene` here.
        CurrentScene = new Scene(Camera.Main, new Skybox(
            skyboxFaces,
            new Shader(
                "Resources\\Shaders\\SkyboxShader.vert", 
                "Resources\\Shaders\\SkyboxShader.frag"
            )
        )); // Replace this with your Scene loading implementation.

        // Tab Panels initialization
        tabPanels.Add(new Viewport()); // Passing the Editor reference ensures better extensibility.
        tabPanels.Add(new Inspector());
        tabPanels.Add(new Hierarchy());
        tabPanels.Add(new Game());
        tabPanels.Add(new AssetBrowser());

        // Start Panels
        foreach (var tabPanel in tabPanels)
            tabPanel.Start(this); // Pass reference explicitly for better encapsulation.

        timer = new Stopwatch();
        timer.Start();

        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Lequal);
    }

    // Graceful cleanup of resources (like tabPanels or OpenGL objects).
    protected override void OnUnload()
    {
        base.OnUnload();

        // Stop or Dispose scene-related resources explicitly.
        foreach (var panel in tabPanels)
            panel.Dispose(); // Ensure TabPanels implement IDisposable.
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // Example Fix: Skip updates if `PLAYING` is false or no scene is loaded.
        if (CurrentScene == null || !PLAYING) return;

        // Update statics
        #region Statics
        Input.KeyboardState = KeyboardState;
        Input.MouseState = MouseState;

        Time.DeltaTime = args.Time;
        Time.ElapsedTime = timer.ElapsedMilliseconds;
        #endregion

        // Game/Viewport functionality
        if (KeyboardState.IsKeyPressed(Keys.Escape))
        {
            if (PLAYING)
            {
                CursorState = CursorState == CursorState.Normal
                    ? CursorState.Grabbed
                    : CursorState.Normal;
            }
            else
            {
                Close(); // Fix: Prevent accidental shutdown during editing.
            }
        }

        CurrentScene.UpdateEditor();

        // Adjust cursor state based on focus
        if (Viewport.IS_VIEWPORT_FOCUSED && Input.MouseState.IsButtonDown(MouseButton.Right))
        {
            CursorState = CursorState.Grabbed;
        }
        else
        {
            CursorState = CursorState.Normal;
        }

        // Update Tab Panels
        foreach (var panel in tabPanels)
            panel.Update();

        if (PLAYING)
        {
            // Fix: Only update live gameplay during PLAYING mode.
            CurrentScene.Update();
        }
    }

    // Overriding the Render Cycle
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        editorRenderer.Render(this, args); // Ensure the EditorRenderer handles errors.

        RenderEditor(); // Fix: Added explicit render call for editor panels.

        SwapBuffers(); // Fix: Ensure buffers are swapped after each rendering cycle.
    }

    // Rendering the Editor
    private void RenderEditor()
    {
        ImGui.DockSpaceOverViewport();

        // Render individual tabs below the viewport.
        foreach (var tabPanel in tabPanels)
            tabPanel.Render();
    }

    // Fix: Proper resizing handling.
    protected override void OnResize(ResizeEventArgs args)
    {
        base.OnResize(args);

        GL.Viewport(0, 0, args.Width, args.Height);

        editorRenderer.Resize(ClientSize); // Use updated client size for resizing renderer.
    }

    protected override void OnTextInput(TextInputEventArgs args)
    {
        base.OnTextInput(args);
        controller.PressChar((char)args.Unicode);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs args)
    {
        base.OnMouseWheel(args);
        controller.MouseScroll(args.Offset);
    }
    */
    #endregion
}