namespace PegasusEngine.Objects.Components;

/// <summary>
/// Base class for all components that can be attached to a <see cref="GameObject"/>.
/// </summary>
public abstract class Component : EngineObject
{
    /// <summary>
    /// Gets the <see cref="GameObject"/> this component is attached to.
    /// </summary>
    public GameObject GameObject { get; internal set; }
    
    /// <summary>
    /// Gets the <see cref="Objects.Transform"/> associated with this component's <see cref="GameObject"/>.
    /// </summary>
    public Transform Transform => GameObject.Transform;
}
