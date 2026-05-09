namespace PegasusEngine.Core.Events;

public enum EventType
{
    // InputEvents
    KeyPress,
    KeyRelease,
    KeyRepeat,
    
    MouseMove,
    MouseButtonPress,
    MouseButtonRelease,
    MouseScroll,
    
    // EngineEvents
    NewFrameRendered,
    UpdateRenderSettings,
    
    WindowResize,
    ReloadedScriptAssemblies,
    
    EventCount
}

public interface IEvent
{
    EventType GetEventType();
    bool IsInputEvent { get; }
    bool IsConsumed { get; }
    void Consume();
}

public abstract class EventBase : IEvent
{
    public abstract EventType GetEventType();

    public virtual bool IsInputEvent => false;
    
    public bool IsConsumed { get; private set; }

    public void Consume() => IsConsumed = true;
    
    public override string ToString() => GetEventType().ToString();
}