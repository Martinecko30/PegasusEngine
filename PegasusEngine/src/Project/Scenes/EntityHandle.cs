using PegasusEngine.Core;
using PegasusEngine.Scripting;

namespace PegasusEngine.Project.Scenes;

public class EntityHandle
{
    public GUID Guid { get; private set; }
    public string Tag = string.Empty;
    public string Name = "Empty Entity";

    private List<Component> _components { get; } = new();
    public IReadOnlyList<Component> Components => _components;
    
    public EntityHandle() => Guid = new();
    public EntityHandle(string name) : this() => Name = name;
    public EntityHandle(string name, string tag) : this(name) => Tag = tag;
    public EntityHandle(GUID guid) => Guid = guid;
    public EntityHandle(GUID guid, string name) : this(guid) => Name = name;
    public EntityHandle(GUID guid, string name, string tag) : this(guid, name) => Tag = tag;

    public EntityHandle(EntityHandle otherHandle) : this(otherHandle.Guid, otherHandle.Name, otherHandle.Tag) {}


    public void AddComponent(Component component) => _components.Add(component);

    
    public T AddComponent<T>() where T : Component, new()
    {
        var component = new T();
        _components.Add(component);
        return component;
    }

    public T? GetComponent<T>() where T : Component
    {
        foreach (var component in _components)
            if (component is T)
                return component as T;
        return null;
    }
    
    public T AddOrGetComponent<T>() where T : Component, new() => GetComponent<T>() ?? AddComponent<T>();
    
    public void TryGetComponent<T>(out T? component) where T : Component => component = GetComponent<T>();
    
    public void RemoveComponent<T>() where T : Component => _components.Remove(GetComponent<T>());
    
    public bool HasComponent<T>() where T : Component => GetComponent<T>() != null;
    
    public override string ToString() => $"Entity: {Name} ({Guid})";
}