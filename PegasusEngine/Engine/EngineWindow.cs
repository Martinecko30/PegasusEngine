using System.Diagnostics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PegasusEngine.Editor.ImGUI;
using PegasusEngine.Editor.Tabs;
using PegasusEngine.Engine.Core;
using PegasusEngine.Engine.Objects;
using PegasusEngine.Engine.Scenes;
using PegasusEngine.Engine.Scripting;
using PegasusEngine.Engine.Shaders;
using PegasusEngine.Engine.Utils;

namespace PegasusEngine.Engine;

public class EngineWindow : GameWindow
{
    private ImGuiController controller;
    
    private List<TabPanel> tabPanels = new List<TabPanel>();

    private readonly bool enabledEditor;
    public static bool DEBUG { private set; get; }
    public static bool PLAYING { private set; get; }
    
    
    public static Scene CurrentScene { private set; get; }
    private Stopwatch timer;
    
    public EngineWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, List<string> args) : base(gameWindowSettings, nativeWindowSettings)
    {
        // Process arguments
        enabledEditor = args.Contains("--editor");
        DEBUG = args.Contains("--debug");
        
        PLAYING = !enabledEditor;
        
        GLFW.WindowHint(WindowHintInt.Samples, 4);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
        // ImGui setup
        controller = new ImGuiController(ClientSize.X / 2, ClientSize.Y / 2);
        
        // Tab Panels
        tabPanels.Add(new Viewport());
        tabPanels.Add(new Inspector());
        tabPanels.Add(new Hierarchy());
        
        List<string> skyboxFaces = new List<string>
        {
            "Resources\\Skybox\\right.jpg",
            "Resources\\Skybox\\left.jpg",
            "Resources\\Skybox\\top.jpg",
            "Resources\\Skybox\\bottom.jpg",
            "Resources\\Skybox\\front.jpg",
            "Resources\\Skybox\\back.jpg"
        };
        
        CurrentScene = new Scene(
            new Camera(new Vector3(0, 20, 0), Size.X / (float)Size.Y),
            new Skybox(
                skyboxFaces, 
                new Shader(
                    "Resources\\Shaders\\SkyboxShader.vert", 
                    "Resources\\Shaders\\SkyboxShader.frag"
                )
        ));

        var defaultShader = new Shader(
            "Resources\\Shaders\\DefaultShader.vert",
            "Resources\\Shaders\\DefaultShader.frag"
            );
        
        var cube = new GameObject(
            "Cube",
            "Resources\\Models\\cube.obj",
            Vector3.Zero, 
            Vector3.One,
            defaultShader
            );
        
        CurrentScene.AddObject(cube);
        
        CursorState = CursorState.Normal;
        
        timer = new Stopwatch();
        timer.Start();
        
        
        // Start Panels
        foreach (TabPanel tabPanel in tabPanels)
            tabPanel.Start(this);
        
        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Lequal);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        var timeStart = timer.ElapsedMilliseconds;
        base.OnRenderFrame(args);
        
        controller.Update(this, (float)args.Time);

        GL.ClearColor(new Color4(0, 32, 48, 255));
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        
        if (enabledEditor)
            RenderEditor();
        
        RenderScene();
        
        if (enabledEditor)
            controller.Render();
        
        ImGuiController.CheckGLError("End of frame");

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (KeyboardState.IsKeyPressed(Keys.Escape) && PLAYING)
        {
            if (CursorState != CursorState.Normal)
                CursorState = CursorState.Normal;
            else
                Environment.Exit(0);
        }
    
        if (!PLAYING)
            return;

        CurrentScene.Update();
    }

    private void RenderEditor()
    {
        ImGui.DockSpaceOverViewport();
        
        // Render
        foreach (TabPanel tabPanel in tabPanels)
            tabPanel.Render();
    }

    private void RenderScene()
    {
        GL.Enable(EnableCap.DepthTest);
        // GL.DepthFunc(DepthFunction.Less);
        // GL.Enable(EnableCap.FramebufferSrgb);

        if (enabledEditor)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Viewport.FBO);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        CurrentScene.Render();
        
        if (enabledEditor)
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
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

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        
        controller.WindowResized(ClientSize.X, ClientSize.Y);
    }
}