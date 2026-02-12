using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PegasusEngine.Pegasus.Core;
using PegasusEngine.Pegasus.Project.Scenes.Components;

namespace PegasusEngine.Pegasus.Project.Scenes;

public class Scene
{
    public const string Extension = ".pgscene";

    public GUID Guid { get; set; } = new GUID();
    public string Name { get; set; } = "Untitled Scene";

    public GUID SkyboxGuid { get; set; } = GUID.INVALID;
    public string SkyboxName { get; set; } = string.Empty;

    public Dictionary<GUID, EntityHandle> Entities { get; } = new();

    public EntityHandle CreateEntity(string name = "Empty Entity")
    {
        return CreateEntity(new GUID(), name);
    }

    public EntityHandle CreateEntity(GUID guid, string name = "Empty Entity")
    {
        var entity = new EntityHandle(guid, name);
        Entities[entity.Guid] = entity;
        return entity;
    }
    
    public void RemoveEntity(EntityHandle entity)
    {
        RemoveEntity(entity.Guid);
    }
    
    public void RemoveEntity(GUID guid)
    {
        Entities.Remove(guid);
    }


    public virtual void OnStart() { }
    public virtual void OnUpdate() { }
    public virtual void OnShutdown() { }

    public static Scene Copy(Scene other)
    {
        var newScene = new Scene
        {
            Guid = other.Guid,
            Name = other.Name,
            SkyboxGuid = other.SkyboxGuid,
            SkyboxName = other.SkyboxName
        };

        // Deep copy entities
        foreach (var oldHandle in other.Entities.Values)
        {
            var otherHandle = new EntityHandle(oldHandle);
            var newHandle = newScene.CreateEntity(otherHandle.Guid, otherHandle.Name);

            foreach (var component in oldHandle.Components)
                newHandle.AddComponent(component);
        }

        return newScene;
    }
}

public static class SceneSerializer
{
    private static readonly ISerializer Serializer;
    private static readonly IDeserializer Deserializer;

    static SceneSerializer()
    {
        var serializerBuilder = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance);
        
        var deserializerBuilder = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance);
        
        var componentTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(Component).IsAssignableFrom(p) && p is { IsInterface: false, IsAbstract: false });

        foreach (var type in componentTypes)
        {
            serializerBuilder.WithTagMapping($"!{type.Name}", type);
            deserializerBuilder.WithTagMapping($"!{type.Name}", type);
        }
        
        Serializer = serializerBuilder.Build();
        Deserializer = deserializerBuilder.Build();
    }

    public static bool Save(string path, Scene scene)
    {
        try
        {
            var data = new SceneDataLayout
            {
                SceneGuid = (ulong)scene.Guid,
                SceneName = scene.Name,
                SkyboxGuid = (ulong)scene.SkyboxGuid,
                SkyboxName = scene.SkyboxName,
                Entities = new List<EntityDataLayout>()
            };

            foreach (var handle in scene.Entities.Values)
            {
                var entityData = new EntityDataLayout
                {
                    Name = handle.Name,
                    Guid = handle.Guid,
                    Tag = handle.Tag,
                    Components = new List<object>(handle.Components)
                };

                data.Entities.Add(entityData);
            }

            string yaml = Serializer.Serialize(data);
            File.WriteAllText(path, yaml);
            Log.EngineInfo("SaveSceneFile: successfully saved scene to {0}", path);
            return true;
        }
        catch (Exception e)
        {
            Log.EngineError("SaveSceneFile: failed to save {0}. {1}", path, e.Message);
            return false;
        }
    }

    public static Scene? Load(string path)
    {
        if (!File.Exists(path)) return null;

        try
        {
            string yaml = File.ReadAllText(path);
            var data = Deserializer.Deserialize<SceneDataLayout>(yaml);

            var scene = new Scene
            {
                Guid = new GUID(data.SceneGuid),
                Name = data.SceneName,
                SkyboxGuid = new GUID(data.SkyboxGuid),
                SkyboxName = data.SkyboxName
            };

            foreach (var eData in data.Entities)
            {
                GUID guid = new GUID(eData.Guid != GUID.INVALID ? eData.Guid : new GUID());
                var name = eData.Name ?? "Unnamed Entity";
                var entity = scene.CreateEntity(guid, name);
                entity.Tag = eData.Tag ?? string.Empty;

                foreach (var component in eData.Components)
                {
                    if (component is Component comp)
                        entity.AddComponent(comp);
                }
            }

            return scene;
        }
        catch (Exception e)
        {
            Log.EngineError("LoadSceneFile: failed to load {0}. {1}", path, e.Message);
            return null;
        }
    }

    // Helper classes to define the YAML structure
    private class SceneDataLayout
    {
        public ulong SceneGuid { get; set; }
        public string SceneName { get; set; } = string.Empty;
        public ulong SkyboxGuid { get; set; }
        public string SkyboxName { get; set; } = string.Empty;
        public List<EntityDataLayout> Entities { get; set; } = new();
    }

    private class EntityDataLayout
    {
        public string? Name { get; set; }
        public string? Tag { get; set; }
        public ulong Guid { get; set; }
        public List<object> Components { get; set; } = new();
    }
}