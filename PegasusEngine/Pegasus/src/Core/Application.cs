using PegasusEngine.Pegasus.Core.Layers;

namespace PegasusEngine.Pegasus.Core;

public class Application
{
    protected IWindow _Window;
    protected LayerStack _LayerStack;
    // protected IRendererAPI _RendererAPI;
    protected Profiler _Profiler;
    //
    // protected ProjectManager _ProjectManager;
    //
    // protected RenderLayer _RenderLayer;
    
    public Application(WindowProps windowProps)
    {
        Log.Init();
        Log.EngineInfo("C# version: {0}", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);

        _Profiler = new Profiler(500);
        
        _Window = IWindow.CreateWindow(windowProps);
        _LayerStack = new LayerStack();
        // _RendererAPI = IRendererAPI.Create();
        // _RendererAPI.Init();
        // _RenderLayer = new _RenderLayer(_LayerStack, _Profiler, _ProjectManager);
        // _LayerStack.PushLayer(_RenderLayer);
    }

    public void Shutdown()
    {
        _LayerStack.OnDetach();
    }

    public void Run()
    {
        while (!_Window.ShouldClose())
        {
            using (var globalTimer = _Profiler.CreateGlobalTimer("GLOBAL"))
            {
                using (var t = _Profiler.CreateTimer("Window.OnUpdate()"))
                    _Window.OnUpdate();
            }

            // _RendererAPI.Clear(new Color4(0.0f, 0.0f, 0.0f, 1.0f)); // black
            
            using (var t = _Profiler.CreateTimer("LayerStack.OnUpdate()"))
            {
                _LayerStack.OnUpdate();
            }
        }

        Shutdown();
    }
}