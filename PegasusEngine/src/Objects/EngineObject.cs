using PegasusEngine.Common;

namespace PegasusEngine.Objects;

/// <summary>
/// Represents the base type for serializable engine objects.
/// </summary>
/// <remarks>
/// Provides a globally unique identifier that can be used by the engine to reference,
/// serialize, and track objects across systems.
/// </remarks>
[Serializable]
public class EngineObject
{
    /// <summary>
    /// Gets the globally unique identifier assigned to this engine object.
    /// </summary>
    /// <remarks>
    /// The identifier is intended to remain stable for the lifetime of the object and may be
    /// used during serialization, asset referencing, or object lookup.
    /// </remarks>
    public GUID Guid { get; protected set; }
}