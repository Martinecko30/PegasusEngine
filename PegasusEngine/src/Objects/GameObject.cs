using System;
using System.Collections.Generic;
using System.Linq;
using PegasusEngine.Core;
using PegasusEngine.Objects.Components;

namespace PegasusEngine.Objects;

[RequireComponent(typeof(Transform))]
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
    }
    public GameObject(GUID guid, string name) : this(guid) => Name = name;
    public GameObject(GUID guid, string name, string tag) : this(guid, name) => Tag = tag;
    public GameObject(GameObject previous) : this(previous.Guid, previous.Name) {}
    
    
    
    public void AddComponent(Component component)
    {
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
        if (typeof(T) == typeof(Transform))
            throw new ArgumentException("Transform cannot be added to GameObject!");

        var component = new T
        {
            GameObject = this
        };
        AddComponent(component);
        return component;
    }
    
    /// <summary>
    /// Gets the first component by type.
    /// If the component is not found, default is returned.
    /// </summary>
    /// <typeparam name="T">Type of component to find.</typeparam>
    /// <returns>The found component.</returns>
    public T? GetComponentByType<T>() where T : Component => components.OfType<T>().FirstOrDefault();
    
    /// <summary>
    /// Tries to get the first component by type.
    /// This method is preferred over GetComponentByType because it returns false if the component is not found.
    /// </summary>
    /// <param name="component">The required component.</param>
    /// <typeparam name="T">Type of component to get.</typeparam>
    /// <returns></returns>
    public bool TryGetComponentByType<T>(out T? component) where T : Component
    {
        component = null;
        foreach (var comp in components)
        {
            if (comp is T compT)
            {
                component = compT;
                return true;
            }
        }

        return false;
    }
    
    public T GetOrAddComponent<T>() where T : Component, new() => GetComponentByType<T>() ?? AddComponent<T>();
    
    public bool HasComponent<T>() where T : Component => TryGetComponentByType<T>(out _);
    
    /// <summary>
    /// Removes component from the GameObject.
    /// If the component is not attached to the GameObject, an exception is thrown.
    /// </summary>
    /// <param name="component">Component to remove</param>
    /// <exception cref="NullReferenceException">If the component is not under this object, an exception is thrown.</exception>
    public void RemoveComponent(Component component) 
    {
        if (!components.Contains(component))
            throw new NullReferenceException("Component is not attached to this GameObject!");
        components.Remove(component);
    }
    
    /// <summary>
    /// Removes the first component by type.
    /// If the component is not found, nothing happens.
    /// This method is preferred over RemoveComponent because it does not throw an exception.
    /// </summary>
    /// <typeparam name="T">Type of component to remove.</typeparam>
    public void RemoveComponentByType<T>() where T : Component
    {
        if (TryGetComponentByType<T>(out var component))
            components.Remove(component);
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
