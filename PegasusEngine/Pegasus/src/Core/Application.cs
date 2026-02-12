using System.Diagnostics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PegasusEngine.Pegasus.Core.Events;
using PegasusEngine.Pegasus.Core.Layers;
using PegasusEngine.Pegasus.Platform.OpenGL;
using PegasusEngine.Pegasus.Project;
using PegasusEngine.Pegasus.Renderer;
using NativeWindow = OpenTK.Windowing.Desktop.NativeWindow;

namespace PegasusEngine.Pegasus.Core;

public class Application
{
    protected GameWindow _Window;
    protected LayerStack _LayerStack;
    protected IRendererAPI _RendererAPI;
    protected Profiler _Profiler;
    
    protected ProjectManager _ProjectManager;

    protected RenderLayer _RenderLayer;
    
    private bool _shouldClose = false;
    
    // TODO: Finish
    public Application(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    {
        Log.Init();
        Log.EngineInfo("C# version: {0}", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);

        _Window = new GameWindow(gameWindowSettings, nativeWindowSettings);
        _Window.Closing += _ => _shouldClose = true;
        
        _Profiler = new Profiler(500);
        
        _ProjectManager = new ProjectManager();
        
        _LayerStack = new LayerStack();
        
        _RendererAPI = new OpenGLRendererAPI();
        _RendererAPI.Init();
        
        _RenderLayer = new RenderLayer(_LayerStack, _Profiler, _ProjectManager);
        _LayerStack.PushLayer(_RenderLayer);
    }

    public void Shutdown()
    {
        _LayerStack.OnDetach();
        _Window.Close();
    }

    public void Run()
    {
        var stopwatch = Stopwatch.StartNew();
        double lastTime = stopwatch.Elapsed.TotalSeconds;
        
        while (!_shouldClose && !_Window.IsExiting)
        {
            double now = stopwatch.Elapsed.TotalSeconds;
            float dt = (float)(now - lastTime);
            lastTime = now;
            
            _Window.MakeCurrent();
            
            using (var globalTimer = _Profiler.CreateGlobalTimer("GLOBAL"))
            {
                using (var t = _Profiler.CreateTimer("Window.OnUpdate()"))
                    NativeWindow.ProcessWindowEvents(false);
                
                _Window.NewInputFrame();
            }

            var fb = _Window.FramebufferSize;
            _RendererAPI.SetViewportSize(fb.X, fb.Y);
            
            _RendererAPI.Clear(new Color4(1.0f, 0.0f, 1.0f, 1.0f)); // magenta
            
            using (var t = _Profiler.CreateTimer("LayerStack.OnUpdate()"))
            {
                _LayerStack.OnUpdate(dt);
            }
            
            _Window.SwapBuffers();
        }
        
        _Window.Dispose();
    }
}