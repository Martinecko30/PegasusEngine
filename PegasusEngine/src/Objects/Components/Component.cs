namespace PegasusEngine.Objects.Components;

public abstract class Component : EngineObject
{
    public GameObject GameObject { get; internal set; }
    public Transform Transform {get; internal set; }
}
