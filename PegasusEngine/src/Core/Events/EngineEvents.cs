using OpenTK.Mathematics;
using PegasusEngine.Renderer.Textures;

// For Vector2

namespace PegasusEngine.Core.Events;

// Note: You'll need to define IImage2D and RenderSettings in C# as well
public class NewFrameRenderedEvent : EventBase
{
    public Texture2D Frame { get; }

    public NewFrameRenderedEvent(Texture2D frame)
    {
        Frame = frame;
    }

    public override EventType GetEventType() => EventType.NewFrameRendered;
}

public class UpdateRenderSettingsEvent : EventBase
{
    public object RenderSettings { get; } // Replace 'object' with your RenderSettings struct/class

    public UpdateRenderSettingsEvent(object renderSettings)
    {
        RenderSettings = renderSettings;
    }

    public override EventType GetEventType() => EventType.UpdateRenderSettings;
}

public class WindowResizeEvent : EventBase
{
    public Vector2i WindowSize { get; }
    
    public int Width => WindowSize.X;
    public int Height => WindowSize.Y;

    public WindowResizeEvent(int width, int height)
    {
        WindowSize = new Vector2i(width, height);
    }

    public override EventType GetEventType() => EventType.WindowResize;
}

public class ReloadScriptAssembliesEvent : EventBase
{
    public readonly List<Type> newScriptTypes;
    public ReloadScriptAssembliesEvent(List<Type> newScriptTypes)
    {
        this.newScriptTypes = newScriptTypes;
    }

    public override EventType GetEventType() => EventType.ReloadedScriptAssemblies;
}