namespace PegasusEngine.Objects.Components;

/// <summary>
/// Represents the base type for engine objects that can be attached to a <see cref="GameObject"/>.
/// </summary>
/// <remarks>
/// Components provide reusable behavior or data for a <see cref="GameObject"/>. Each component belongs to
/// exactly one game object and can access that object's transform through <see cref="Transform"/>.
/// </remarks>
public abstract class Component : EngineObject
{
    /// <summary>
    /// Gets the <see cref="GameObject"/> that owns this component.
    /// </summary>
    /// <remarks>
    /// This property is assigned by the engine when the component is added to a game object.
    /// </remarks>
    public GameObject GameObject { get; internal set; }
    
    /// <summary>
    /// Gets the <see cref="Objects.Transform"/> of the <see cref="GameObject"/> that owns this component.
    /// </summary>
    /// <remarks>
    /// This is a convenience accessor for <c>GameObject.Transform</c>.
    /// </remarks>
    public Transform Transform => GameObject.Transform;
}
