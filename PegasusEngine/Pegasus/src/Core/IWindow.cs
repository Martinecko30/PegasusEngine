using OpenTK.Mathematics;
using PegasusEngine.Pegasus.Core.Events;

namespace PegasusEngine.Pegasus.Core;

public struct WindowProps
{
    string Title;
    int Width, Height;
    bool VSync;
    bool CustomTitlebar;

    WindowProps(
        string title = "Pegasus Engine", 
        int width = 1280, 
        int height = 720, 
        bool vSync = false, 
        bool customTitlebar = false
        )
    {
        Title = title;
        Width = width;
        Height = height;
        VSync = vSync;
        CustomTitlebar = customTitlebar;
    }
}

public interface IWindow
{
    void OnUpdate();
    
    void SetTitle(string title);
    
    Vector2i GetFramebufferSize();
    
    IntPtr GetNativeWindow();
    
    bool IsVSync { get; set; }
    
    bool IsFullscreen { get; set; }

    void Minimize();
    void Maximize();
    void Restore();
    bool IsMinimized();
    void Close();
    void SetPosition(int x, int y);
    Vector2i GetPosition();

    void SetTitlebarHitTestCallback(Func<int, int, bool> callback);

    bool IsKeyPressed(int key);
    bool IsMouseButtonPressed(int button);
    (float X, float Y) GetMousePosition();

    bool ShouldClose();

    void SetEventCallback(Action<IEvent> callback);

    static IWindow CreateWindow(WindowProps windowProps)
    {
        // This would typically return a platform-specific implementation
        // e.g., return new WindowsWindow(windowProps);
        throw new NotImplementedException("Platform-specific window implementation not found.");
    }
}