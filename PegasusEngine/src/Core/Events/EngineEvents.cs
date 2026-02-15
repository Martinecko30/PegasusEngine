using System.Numerics;
// For Vector2

namespace PegasusEngine.Core.Events;

// Note: You'll need to define IImage2D and RenderSettings in C# as well
public class NewFrameRenderedEvent : EventBase
{
    public object Frame { get; } // Replace 'object' with your IImage2D interface

    public NewFrameRenderedEvent(object frame)
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
    public Vector2 WindowSize { get; }

    public WindowResizeEvent(int width, int height)
    {
        WindowSize = new Vector2(width, height);
    }

    public override EventType GetEventType() => EventType.WindowResize;
}