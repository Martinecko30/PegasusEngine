using System.Diagnostics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PegasusEngine.Core.Events;
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
    protected Profiler Profiler;
    
    protected ProjectManager ProjectManager;
    protected ScriptManager ScriptManager;
    
    protected RenderLayer RenderLayer;
    protected ScriptLayer ScriptLayer;
    
    protected IRenderer Renderer;
    
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

        Renderer = new Renderer.Renderer(ProjectManager.AssetManager);

        RenderLayer = new RenderLayer(LayerStack, Profiler, ProjectManager, Renderer);
        LayerStack.PushLayer(RenderLayer);
        
        
        ScriptLayer = new ScriptLayer(ScriptManager, ProjectManager, LayerStack);
        LayerStack.PushLayer(ScriptLayer);
        
        // Setup callbacks
        Window.Closing += _ => _shouldClose = true;
        Window.Resize += e => LayerStack.DispatchEvent(new WindowResizeEvent(e.Width, e.Height));
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