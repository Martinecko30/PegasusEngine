namespace PegasusEngine.Core.Events;

public abstract class KeyEvent : EventBase
{
    public KeyCode Key { get; }
    public bool Ctrl { get; }
    public bool Shift { get; }
    public bool Alt { get; }
    public bool Super { get; }
    
    public override bool IsInputEvent => true;
    
    protected KeyEvent(KeyCode key, bool ctrl, bool shift, bool alt, bool super)
    {
        Key = key;
        Ctrl = ctrl;
        Shift = shift;
        Alt = alt;
        Super = super;
    }
}

public class KeyPressEvent : KeyEvent
{
    public KeyPressEvent(KeyCode key, bool ctrl, bool shift, bool alt, bool super)
        : base(key, ctrl, shift, alt, super) {}
    
    public override EventType GetEventType() => EventType.KeyPress;
}

public class KeyReleaseEvent : KeyEvent
{
    public KeyReleaseEvent(KeyCode key, bool ctrl, bool shift, bool alt, bool super)
        : base(key, ctrl, shift, alt, super) {}
    
    public override EventType GetEventType() => EventType.KeyRelease;
}

public class KeyRepeatEvent : KeyEvent
{
    public KeyRepeatEvent(KeyCode key, bool ctrl, bool shift, bool alt, bool super)
        : base(key, ctrl, shift, alt, super) {}
    
    public override EventType GetEventType() => EventType.KeyRepeat;
}

public class MouseMoveEvent : EventBase
{
    public double X { get; }
    public double Y { get; }
    
    public MouseMoveEvent(double x, double y)
    {
        X = x;
        Y = y;
    }
    
    public override EventType GetEventType() => EventType.MouseMove;
}

public class MouseButtonPressEvent : EventBase
{
    public MouseCode Button { get; }
    public override bool IsInputEvent => true;

    public MouseButtonPressEvent(MouseCode button) => Button = button;

    public override EventType GetEventType() => EventType.MouseButtonPress;
}

public class MouseButtonReleaseEvent : EventBase
{
    public MouseCode Button { get; }
    public override bool IsInputEvent => true;

    public MouseButtonReleaseEvent(MouseCode button) => Button = button;

    public override EventType GetEventType() => EventType.MouseButtonRelease;
}

public class MouseScrollEvent : EventBase
{
    public double XOffset { get; }
    public double YOffset { get; }
    public override bool IsInputEvent => true;

    public MouseScrollEvent(double xOffset, double yOffset)
    {
        XOffset = xOffset;
        YOffset = yOffset;
    }

    public override EventType GetEventType() => EventType.MouseScroll;
}