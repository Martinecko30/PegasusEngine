using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PegasusEngine.Common;
using PegasusEngine.Core;
using PegasusEngine.Objects;
using PegasusEngine.Objects.Components;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PegasusEngine.Project.Scenes;

public class Scene
{
    public GUID Guid { get; set; } = new GUID();
    public string Name { get; set; } = "Untitled Scene";

    public GUID SkyboxGuid { get; set; } = GUID.INVALID;
    public string SkyboxName { get; set; } = string.Empty;

    public Dictionary<GUID, GameObject> Entities { get; } = new();

    public void AddEntity(GameObject entity)
    {
        Entities[entity.Guid] = entity;
    }

    public void AddEntities(IEnumerable<GameObject> entities)
    {
        foreach (var entity in entities)
            Entities[entity.Guid] = entity;
    }

    public GameObject CreateEntity(string name = "Empty Entity")
    {
        return CreateEntity(new GUID(), name);
    }

    public GameObject CreateEntity(GUID guid, string name = "Empty Entity")
    {
        var entity = new GameObject(guid, name);
        Entities[entity.Guid] = entity;
        return entity;
    }
    
    public void RemoveEntity(GameObject entity)
    {
        RemoveEntity(entity.Guid);
    }
    
    public void RemoveEntity(GUID guid)
    {
        Entities.Remove(guid);
    }
    
    /// <summary>
    /// Extremely fast O(1) lookup by GUID. 
    /// Used heavily by the Editor and Reference Resolvers.
    /// </summary>
    public GameObject? Find(GUID guid)
    {
        if (guid == GUID.INVALID) return null;
        return Entities.TryGetValue(guid, out var entity) ? entity : null;
    }

    /// <summary>
    /// Finds the first active GameObject in the scene by its exact name.
    /// Note: This is an O(N) operation. Do not call this inside OnUpdate!
    /// </summary>
    public GameObject? Find(string name)
    {
        foreach (var entity in Entities.Values)
        {
            // Using Ordinal comparison for maximum engine performance
            if (entity.Name.Equals(name, StringComparison.Ordinal))
                return entity;
        }
        return null;
    }

    /// <summary>
    /// Finds the first GameObject in the scene with the specified tag.
    /// Highly useful for finding the "Player" or "MainCamera".
    /// </summary>
    public GameObject? FindWithTag(string tag)
    {
        foreach (var entity in Entities.Values)
        {
            if (entity.CompareTag(tag))
                return entity;
        }
        return null;
    }


    public virtual void OnStart() { }
    public virtual void OnUpdate() { }
    public virtual void OnShutdown() { }
}