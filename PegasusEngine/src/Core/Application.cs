using System.Diagnostics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PegasusEngine.Core.Layers;
using PegasusEngine.Debug;
using PegasusEngine.Platform.OpenGL;
using PegasusEngine.Project;
using PegasusEngine.Renderer;
using PegasusEngine.Scripting;
using NativeWindow = OpenTK.Windowing.Desktop.NativeWindow;

namespace PegasusEngine.Core;

public class Application
{
    protected GameWindow Window;
    protected LayerStack LayerStack;
    protected IRendererAPI RendererApi;
    protected Profiler Profiler;
    
    protected ProjectManager ProjectManager;
    protected ScriptManager ScriptManager;
    
    protected RenderLayer RenderLayer;

    protected ScriptLayer ScriptLayer;
    
    private bool _shouldClose = false;
    
    // TODO: Finish
    public Application(
        GameWindowSettings gameWindowSettings,
        NativeWindowSettings nativeWindowSettings)
    {
        Log.Init();
        Log.EngineInfo("C# version: {0}", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);

        Window = new GameWindow(gameWindowSettings, nativeWindowSettings);
        
        Profiler = new Profiler(500);
        
        ProjectManager = new ProjectManager();
        ScriptManager = new ScriptManager();
        
        LayerStack = new LayerStack();
        
        RendererApi = new OpenGLRendererAPI();
        RendererApi.Init();


        RenderLayer = new RenderLayer(LayerStack, Profiler, ProjectManager);
        LayerStack.PushLayer(RenderLayer);
        
        
        ScriptLayer = new ScriptLayer(ScriptManager, ProjectManager);
        LayerStack.PushLayer(ScriptLayer);
        
        // Setup callbacks
        Window.Closing += _ => _shouldClose = true;
        Window.Resize += e => RendererApi.SetViewportSize(e.Width, e.Height);
    }

    public void Shutdown()
    {
        LayerStack.OnDetach();
        Window.Close();
    }

    public void Run()
    {
        Window.MakeCurrent();
        
        var stopwatch = Stopwatch.StartNew();
        double lastTime = stopwatch.Elapsed.TotalSeconds;

        while (!_shouldClose && !Window.IsExiting)
        {
            double now = stopwatch.Elapsed.TotalSeconds;
            float dt = (float)(now - lastTime);
            lastTime = now;


            using (var globalTimer = Profiler.CreateGlobalTimer("GLOBAL"))
            {
                using (var t = Profiler.CreateTimer("Window.OnUpdate()"))
                    NativeWindow.ProcessWindowEvents(false);
                
                RendererApi.Clear(new Color4(1.0f, 0.0f, 1.0f, 1.0f)); // magenta
                
                using (var t = Profiler.CreateTimer("LayerStack.OnUpdate()"))
                {
                    LayerStack.OnUpdate(dt);
                }
                
                Window.SwapBuffers();
                Window.NewInputFrame();
            }
        }
        
        Window.Dispose();
    }
}