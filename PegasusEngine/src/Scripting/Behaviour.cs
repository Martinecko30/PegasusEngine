using PegasusEngine.Objects.Components;

namespace PegasusEngine.Scripting;

/// <summary>
/// Base class for user-defined script components that participate in the engine lifecycle.
/// </summary>
/// <remarks>
/// Derive from this class to implement gameplay logic. Override lifecycle methods such as
/// <see cref="Start"/> and <see cref="Update"/> to run code at specific points during execution.
/// </remarks>
public abstract class Behaviour : Component
{
    /// <summary>
    /// Called once when the behaviour begins execution.
    /// </summary>
    /// <remarks>
    /// Override this method to perform initialization that depends on the component being attached
    /// to a <see cref="Objects.GameObject"/> and ready for use.
    /// </remarks>
    public virtual void Start() {}
    
    /// <summary>
    /// Called once per engine update while the behaviour is active.
    /// </summary>
    /// <remarks>
    /// Override this method to implement frame-based gameplay logic.
    /// </remarks>
    public virtual void Update() {}
    
    // TODO: Add more lifecycle methods
}