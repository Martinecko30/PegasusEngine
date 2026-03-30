using System;
using System.Collections.Generic;
using System.Linq;
using PegasusEngine.Common;
using PegasusEngine.Core;
using PegasusEngine.Debug;
using PegasusEngine.Objects.Components;

namespace PegasusEngine.Objects;

[RequireComponent(typeof(Transform))]
[Serializable]
public class GameObject : EngineObject
{
    public string Tag = string.Empty;
    public string Name { get; set; } = "Unnamed Object";
    
    public Transform Transform { get; private set; }
    
    public bool IsActive { get; private set; } = true;
    public void SetActive(bool active) => IsActive = active;
    
    private List<Component> components = new();
    public IReadOnlyList<Component> Components => components;
    
    public GameObject(GUID guid)
    {
        Guid = guid;
        
        // TODO: Add Transform component automatically
        Transform = new Transform(this);
        components.Add(Transform);
    }
    public GameObject(GUID guid, string name) : this(guid) => Name = name;
    public GameObject(GUID guid, string name, string tag) : this(guid, name) => Tag = tag;
    public GameObject(GameObject previous) : this(previous.Guid, previous.Name) {}
    
    
    
    public void AddComponent(Component component)
    {
        if (component == null) throw new ArgumentNullException(nameof(component));
        
        if (component is Transform)
            throw new ArgumentException("Cannot add a second Transform to a GameObject!");
        
        Type componentType = component.GetType();
        
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
                    Log.EngineError($"Failed to automatically add required component '{requiredType.Name}' for '{componentType.Name}'. Does it lack a parameterless constructor?");
                    throw;
                }
            }
        }
        
        component.GameObject = this;
        components.Add(component);
    }

    /// <summary>
    /// Adds a component to the GameObject.
    /// If the component is Transform, an exception is thrown.
    /// </summary>
    /// <typeparam name="T">Specific component type to add.</typeparam>
    /// <returns>Reference to the new Component</returns>
    /// <exception cref="ArgumentException">If the new component is Transform.</exception>
    public T AddComponent<T>() where T : Component, new()
    {
        var component = new T();
        AddComponent(component);
        return component;
    }

    /// <summary>
    /// Gets the first component by type.
    /// If the component is not found, default is returned.
    /// </summary>
    /// <typeparam name="T">Type of component to find.</typeparam>
    /// <returns>The found component.</returns>
    public T? GetComponentByType<T>(T? componentType = null) where T : Component
    {
        TryGetComponent<T>(out var component);
        return component;
    }
    
    /// <summary>
    /// Tries to get the first component by type.
    /// This method is preferred over GetComponentByType because it returns false if the component is not found.
    /// </summary>
    /// <param name="component">The required component.</param>
    /// <typeparam name="T">Type of component to get.</typeparam>
    /// <returns></returns>
    public T? GetComponent<T>() where T : Component
    {
        foreach (var comp in components)
        {
            if (comp is T match) return match;
        }
        return null;
    }
    
    public Component? GetComponent(Type type)
    {
        foreach (var comp in components)
        {
            if (type.IsInstanceOfType(comp)) return comp;
        }
        return null;
    }

    public bool TryGetComponent<T>(out T? component) where T : Component
    {
        component = GetComponent<T>();
        return component != null;
    }
    
    public T GetOrAddComponent<T>() where T : Component, new() 
        => GetComponent<T>() ?? AddComponent<T>();
    
    public bool HasComponent<T>() where T : Component 
        => GetComponent<T>() != null;
    
    /// <summary>
    /// Removes component from the GameObject.
    /// If the component is not attached to the GameObject, an exception is thrown.
    /// </summary>
    /// <param name="component">Component to remove</param>
    /// <exception cref="NullReferenceException">If the component is not under this object, an exception is thrown.</exception>
    public void RemoveComponent(Component component) 
    {
        if (component is Transform)
            throw new InvalidOperationException("Cannot remove the Transform from a GameObject!");

        if (!components.Remove(component))
            throw new ArgumentException("Component is not attached to this GameObject!");
            
        component.GameObject = null!;
    }
    
    /// <summary>
    /// Removes the first component by type.
    /// If the component is not found, nothing happens.
    /// This method is preferred over RemoveComponent because it does not throw an exception.
    /// </summary>
    /// <typeparam name="T">Type of component to remove.</typeparam>
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
 
    public override string ToString()
    {
        return Name;
    }
    
    public bool CompareTag(string tag)
    {
        return Tag.Equals(tag);
    }
}
