using PegasusEngine.Common;
using PegasusEngine.Debug;
using PegasusEngine.Objects.Components;

namespace PegasusEngine.Objects;

/// <summary>
/// Represents an entity in the scene that can contain components and participate in engine systems.
/// </summary>
[RequireComponent(typeof(Transform))]
[Serializable]
public class GameObject : EngineObject
{
    /// <summary>
    /// The tag used to identify or group this game object.
    /// </summary>
    public string Tag = string.Empty;
    
    /// <summary>
    /// Gets or sets the display name of this game object.
    /// </summary>
    public string Name { get; set; } = "Unnamed Object";

    /// <summary>
    /// Gets the transform component attached to this game object.
    /// </summary>
    public Transform Transform => GetComponent<Transform>()!;
    
    /// <summary>
    /// Gets whether this game object is currently active.
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>
    /// Sets the active state of this game object.
    /// </summary>
    /// <param name="active">Whether the game object should be active.</param>
    public void SetActive(bool active) => IsActive = active;
    
    private List<Component> components = new();
    
    /// <summary>
    /// Gets the components currently attached to this game object.
    /// </summary>
    public IReadOnlyList<Component> Components => components;
    
    /// <summary>
    /// Creates a new game object with the specified GUID.
    /// </summary>
    /// <param name="guid">The unique identifier assigned to this game object.</param>
    public GameObject(GUID guid)
    {
        Guid = guid;

        AddComponent<Transform>();
    }
    
    /// <summary>
    /// Creates a new game object with the specified GUID and name.
    /// </summary>
    /// <param name="guid">The unique identifier assigned to this game object.</param>
    /// <param name="name">The display name of this game object.</param>
    public GameObject(GUID guid, string name) : this(guid) => Name = name;
    
    /// <summary>
    /// Creates a new game object with the specified GUID, name, and tag.
    /// </summary>
    /// <param name="guid">The unique identifier assigned to this game object.</param>
    /// <param name="name">The display name of this game object.</param>
    /// <param name="tag">The tag assigned to this game object.</param>
    public GameObject(GUID guid, string name, string tag) : this(guid, name) => Tag = tag;
    
    /// <summary>
    /// Creates a new game object by copying identifying values from another game object.
    /// </summary>
    /// <param name="previous">The game object to copy values from.</param>
    public GameObject(GameObject previous) : this(previous.Guid, previous.Name) {}
    
    
    /// <summary>
    /// Adds an existing component instance to this game object.
    /// </summary>
    /// <param name="component">The component instance to attach.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="component"/> is <see langword="null"/>.</exception>
    /// <exception cref="Exception">
    /// Thrown when a required dependency component cannot be automatically created.
    /// </exception>
    public void AddComponent(Component component)
    {
        if (component == null) throw new ArgumentNullException(nameof(component));
        
        Type componentType = component.GetType();
        bool disallowMultiple = componentType.GetCustomAttributes(typeof(DisallowMultipleComponents), true).Length > 0;
    
        if (disallowMultiple && GetComponent(componentType) != null)
        {
            Log.EngineError($"Cannot add multiple instances of '{componentType.Name}' to GameObject '{Name}'. " +
                            "It is restricted by [DisallowMultipleComponent].");
            return;
        }
        
        component.GameObject = this;
        components.Add(component);
        
        var attributes = componentType.GetCustomAttributes(typeof(RequireComponent), true);

        foreach (RequireComponent attribute in attributes)
        {
            Type requiredType = attribute.RequiredComponentType;
            
            if (GetComponent(requiredType) == null)
            {
                try
                {
                    var dependency = (Component)Activator.CreateInstance(requiredType)!;
                    
                    AddComponent(dependency); 
                }
                catch (Exception ex)
                {
                    components.Remove(component);
                    component.GameObject = null!;
                    
                    Log.EngineError($"Failed to automatically add required component '{requiredType.Name}' for '{componentType.Name}'. Does it lack a parameterless constructor?");
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Adds a new component of the specified type to this game object.
    /// </summary>
    /// <typeparam name="T">The component type to create and attach.</typeparam>
    /// <returns>The newly created component instance.</returns>
    public T AddComponent<T>() where T : Component, new()
    {
        var component = new T();
        AddComponent(component);
        return component;
    }

    /// <summary>
    /// Gets the first component of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of component to find.</typeparam>
    /// <param name="componentType">Unused optional component parameter.</param>
    /// <returns>The matching component if found; otherwise, <see langword="null"/>.</returns>
    public T? GetComponentByType<T>(T? componentType = null) where T : Component
    {
        TryGetComponent<T>(out var component);
        return component;
    }
    
    /// <summary>
    /// Gets the first component of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of component to find.</typeparam>
    /// <returns>The matching component if found; otherwise, <see langword="null"/>.</returns>
    public T? GetComponent<T>() where T : Component
    {
        foreach (var comp in components)
        {
            if (comp is T match) return match;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets the first component assignable to the specified runtime type.
    /// </summary>
    /// <param name="type">The runtime component type to find.</param>
    /// <returns>The matching component if found; otherwise, <see langword="null"/>.</returns>
    public Component? GetComponent(Type type)
    {
        foreach (var comp in components)
        {
            if (type.IsInstanceOfType(comp)) return comp;
        }
        return null;
    }

    /// <summary>
    /// Attempts to get the first component of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of component to find.</typeparam>
    /// <param name="component">
    /// When this method returns, contains the matching component if found; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if a component was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetComponent<T>(out T? component) where T : Component
    {
        component = GetComponent<T>();
        return component != null;
    }
    
    /// <summary>
    /// Gets the first component of the specified type, or adds one if none exists.
    /// </summary>
    /// <typeparam name="T">The component type to get or add.</typeparam>
    /// <returns>The existing or newly created component.</returns>
    public T GetOrAddComponent<T>() where T : Component, new() 
        => GetComponent<T>() ?? AddComponent<T>();
    
    /// <summary>
    /// Determines whether this game object has a component of the specified type.
    /// </summary>
    /// <typeparam name="T">The component type to check for.</typeparam>
    /// <returns><see langword="true"/> if the component exists; otherwise, <see langword="false"/>.</returns>
    public bool HasComponent<T>() where T : Component 
        => GetComponent<T>() != null;
    
    /// <summary>
    /// Removes a component from this game object.
    /// </summary>
    /// <param name="component">The component to remove.</param>
    /// <exception cref="InvalidOperationException">Thrown when attempting to remove the transform component.</exception>
    /// <exception cref="ArgumentException">Thrown when the component is not attached to this game object.</exception>
    public void RemoveComponent(Component component) 
    {
        if (component is Transform)
            throw new InvalidOperationException("Cannot remove the Transform from a GameObject!");

        if (!components.Remove(component))
            throw new ArgumentException("Component is not attached to this GameObject!");
            
        component.GameObject = null!;
    }
    
    /// <summary>
    /// Removes the first component of the specified type from this game object.
    /// </summary>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown when attempting to remove the transform component.</exception>
    public void RemoveComponentByType<T>() where T : Component
    {
        if (typeof(T) == typeof(Transform))
            throw new InvalidOperationException("Cannot remove the Transform from a GameObject!");

        if (TryGetComponent<T>(out var component))
        {
            components.Remove(component!);
            component!.GameObject = null!;
        }
    }
 
    /// <summary>
    /// Returns the display name of this game object.
    /// </summary>
    /// <returns>The game object's name.</returns>
    public override string ToString()
    {
        return Name;
    }
    
    /// <summary>
    /// Determines whether this game object's tag matches the specified tag.
    /// </summary>
    /// <param name="tag">The tag to compare against this game object's tag.</param>
    /// <returns><see langword="true"/> if the tags match; otherwise, <see langword="false"/>.</returns>
    public bool CompareTag(string tag)
    {
        return Tag.Equals(tag);
    }
}
