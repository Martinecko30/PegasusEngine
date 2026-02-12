using System.Collections;
using PegasusEngine.Pegasus.Core;
using PegasusEngine.Pegasus.Project.Scenes.Components;
using PegasusEngine.Pegasus.Project.Scenes.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PegasusEngine.Pegasus.Project.Scenes;

public class SceneManager : IEnumerable<KeyValuePair<GUID, Scene>>
{
    public const string SceneFileExtension = ".pgscene";

    private GUID _openSceneGuid = GUID.INVALID;
    private GUID _openSceneGuidCache = GUID.INVALID;
    private bool _inRuntimeSimulation = false;

    private readonly Dictionary<GUID, Scene> _scenes = new();
    private readonly Dictionary<GUID, Scene> _runtimeSimulationScenes = new();
    
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;
    
    public SceneManager()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
    }

    
    private Dictionary<GUID, Scene> ActiveScenes => _inRuntimeSimulation ? _runtimeSimulationScenes : _scenes;

    public GUID CreateScene(string name = "Empty Scene")
    {
        if (_inRuntimeSimulation)
        {
            Log.EngineWarn("CreateScene: Cannot create scenes while in runtime simulation");
            return GUID.INVALID;
        }

        var scene = new Scene { Name = name };
        // Scene constructor should ideally generate a new GUID
        ActiveScenes[scene.Guid] = scene;
        
        Log.EngineInfo("CreateScene: created new scene \"{0}\" with GUID {1}", name, (ulong)scene.Guid);
        return scene.Guid;
    }

    public void DeleteScene(GUID guid)
    {
        if (_inRuntimeSimulation)
        {
            Log.EngineWarn("DeleteScene: Cannot delete scenes while in runtime simulation");
            return;
        }

        if (_openSceneGuid == guid)
        {
            _openSceneGuid = GUID.INVALID;
            Log.EngineInfo("DeleteScene: closed open scene (GUID {0}) because it was deleted", (ulong)guid);
        }

        if (ActiveScenes.Remove(guid))
        {
            Log.EngineInfo("DeleteScene: successfully removed scene with GUID {0}", (ulong)guid);
        }
        else
        {
            Log.EngineWarn("DeleteScene: no scene found with GUID {0}; nothing deleted", (ulong)guid);
        }
    }

    public bool SetOpenSceneGuid(GUID guid)
    {
        var scene = Find(guid);
        if (scene == null && guid != GUID.INVALID)
        {
            Log.EngineWarn("SetOpenScene: cannot open scene; no scene registered with GUID {0}", (ulong)guid);
            return false;
        }

        _openSceneGuid = guid;
        Log.EngineInfo("SetOpenScene: now tracking scene with GUID {0} as the active scene", (ulong)guid);
        return true;
    }

    public GUID GetOpenSceneGuid() => _openSceneGuid;

    public Scene? GetOpenScene() => Find(_openSceneGuid);

    public void EnterRuntimeSimulation()
    {
        if (_inRuntimeSimulation) return;

        _inRuntimeSimulation = true;
        _runtimeSimulationScenes.Clear();

        foreach (var pair in _scenes)
        {
            // Assumes a Scene.Copy(Scene) method exists
            _runtimeSimulationScenes.Add(pair.Key, Scene.Copy(pair.Value));
        }

        _openSceneGuidCache = _openSceneGuid;
    }

    public void ExitRuntimeSimulation()
    {
        if (!_inRuntimeSimulation) return;

        _inRuntimeSimulation = false;
        _runtimeSimulationScenes.Clear();
        _openSceneGuid = _openSceneGuidCache;
    }

    public void SaveScenesToFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        // 1. Delete orphaned .pgscene files
        var files = Directory.GetFiles(folderPath, "*" + SceneFileExtension);
        foreach (var scenePath in files)
        {
            GUID guid = ExtractGuidFromScenepath(scenePath);
            if (guid == GUID.INVALID)
            {
                Log.EngineWarn("SaveScenesToFolder: invalid GUID in filename \"{0}\"; skipping orphaned file", scenePath);
                continue;
            }

            if (Find(guid) == null)
            {
                try
                {
                    File.Delete(scenePath);
                    Log.EngineInfo("SaveScenesToFolder: deleting orphaned scene file \"{0}\" (GUID {1})", scenePath, (ulong)guid);
                }
                catch (Exception e)
                {
                    Log.EngineError("SaveScenesToFolder: failed to delete {0}. {1}", scenePath, e.Message);
                }
            }
        }

        // 2. Serialize in-memory scenes (always save the 'real' ones)
        foreach (var pair in _scenes)
        {
            string scenePath = ComposeScenepathFromGuid(folderPath, pair.Key);
            // Replaces SaveSceneFile call
            if (!SaveSceneFile(scenePath, pair.Value))
            {
                Log.EngineWarn("SaveScenesToFolder: failed to serialize scene GUID {0} to \"{1}\"", (ulong)pair.Key, scenePath);
            }
            else
            {
                Log.EngineInfo("SaveScenesToFolder: successfully saved scene GUID {0} to \"{1}\"", (ulong)pair.Key, scenePath);
            }
        }
    }

    public void LoadScenesFromFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        foreach (var scenePath in Directory.GetFiles(folderPath, "*" + SceneFileExtension))
        {
            // Replaces LoadSceneFile call
            var scene = LoadSceneFile(scenePath);
            if (scene == null)
            {
                Log.EngineWarn("LoadScenesFromFolder: failed to deserialize scene file \"{0}\"; skipping", scenePath);
                continue;
            }

            _scenes[scene.Guid] = scene;
            Log.EngineInfo("LoadScenesFromFolder: loaded scene \"{0}\" with GUID {1}", scenePath, (ulong)scene.Guid);
        }
    }

    public Scene? Find(GUID guid)
    {
        if (guid == GUID.INVALID) return null;
        return ActiveScenes.TryGetValue(guid, out var scene) ? scene : null;
    }

    // Required for C# "foreach (var scene in sceneManager)" support
    public IEnumerator<KeyValuePair<GUID, Scene>> GetEnumerator() => ActiveScenes.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Utilities
    private static GUID ExtractGuidFromScenepath(string scenePath)
    {
        string stem = Path.GetFileNameWithoutExtension(scenePath);
        if (ulong.TryParse(stem, out ulong value))
        {
            return new GUID(value);
        }

        Log.EngineWarn("ExtractGuid: filename \"{0}\" isn't a valid unsigned integer - skipping", scenePath);
        return GUID.INVALID;
    }

    private static string ComposeScenepathFromGuid(string folderPath, GUID guid)
    {
        return Path.Combine(folderPath, (ulong)guid + SceneFileExtension);
    }

    // Mock methods for external serialization - implement these based on your serializer
    private bool SaveSceneFile(string path, Scene scene)
    {
        if (!(Path.HasExtension(path) && Path.GetExtension(path).ToLower() == SceneFileExtension))
        {
            Log.EngineWarn("SaveSceneFile: invalid file extension '{}'.", path);
            return false;
        }

        var parentDir = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(parentDir) || !Directory.Exists(parentDir))
        {
            Log.EngineWarn("SaveSceneFile: parent directory '{}' does not exist.", parentDir ?? "<null>");
            return false;
        }
        
        try
        {
            var componentRegistry = new ComponentYamlRegistry();
            var yaml = SerializeScene(scene, _serializer, componentRegistry);
            
            File.WriteAllText(path, yaml);
            Log.EngineInfo("SaveMetaFile: wrote metadata for GUID {0}.", scene.Guid);
            return true;
        } catch (Exception e)
        {
            Log.EngineError("SaveMetaFile: failed to write metadata for GUID {0}.", scene.Guid);
            Log.EngineError(e.ToString());
            return false;
        }
        
        return false;
    }

    private Scene? LoadSceneFile(string path)
    {
        if (!(Path.HasExtension(path) && Path.GetExtension(path).ToLower() == SceneFileExtension))
        {
            Log.EngineWarn("LoadSceneFile: invalid file extension '{}'.", path);
            return null;
        }

        var parentDir = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(parentDir) || !Directory.Exists(parentDir))
        {
            Log.EngineWarn("LoadSceneFile: parent directory '{}' does not exist.", parentDir ?? "<null>");
            return null;
        }
        
        try
        {
            string yaml = File.ReadAllText(path);
            var scene = DeserializeScene(yaml, _deserializer);
            
            File.WriteAllText(path, yaml);
            Log.EngineInfo("LoadSceneFile: saved scene with GUID {0}.", scene.Guid);
            return scene;
        } catch (Exception e)
        {
            Log.EngineError("LoadSceneFile: failed to read data for scene with GUID {0}.", path);
            Log.EngineError(e.ToString());
            return null;
        }
        
        
        return null;
    }

    private static string SerializeScene(
        Scene scene,
        ISerializer serializer,
        ComponentYamlRegistry registry
        )
    {
        var dto = new SceneFileDto
        {
            SceneGuid = (ulong)scene.Guid,
            SceneName = scene.Name,
            SkyboxGuid = (ulong)scene.SkyboxGuid,
            SkyboxName = scene.SkyboxName
        };

        foreach (var (guid, entity) in scene.Entities)
        {
            var e = new EntityDto
            {
                Guid = (ulong)guid,
                Tag = entity.Tag,
                Name = entity.Name
            };

            foreach (var component in entity.Components)
            {
                if (registry.TryWrite(component, out var c))
                    e.Components.Add(c);
                else
                    Log.EditorWarn("SerializeScene: Unknow component type!");
            }
            
            dto.Entities.Add(e);
        }
        
        return serializer.Serialize(dto);
    }

    private static Scene? DeserializeScene(string yaml, IDeserializer deserializer)
    {
        if (string.IsNullOrWhiteSpace(yaml))
            return null;

        SceneFileDto dto;
        try
        {
            dto = deserializer.Deserialize<SceneFileDto>(yaml);
        }
        catch
        {
            return null;
        }

        var scene = new Scene
        {
            Guid = new GUID(dto.SceneGuid),
            Name = dto.SceneName,
            SkyboxGuid = new GUID(dto.SkyboxGuid),
            SkyboxName = dto.SkyboxName
        };

        var applier = new ReflectionYamlAutoDeserializer();

        foreach (var e in dto.Entities)
        {
            var entity = new EntityHandle(new GUID(e.Guid), e.Name, e.Tag);

            foreach (var c in e.Components)
            {
                var component = CreateComponentInstance(c.Type);
                if (component is null)
                    continue;

                applier.ApplyObjectGraph(component, c.Data);
                entity.AddComponent(component);
            }

            scene.Entities.Add(entity.Guid, entity);
        }

        return scene;
    }
    
    private static Component? CreateComponentInstance(string typeId)
    {
        if (string.IsNullOrWhiteSpace(typeId))
            return null;

        // You wrote Type as AssemblyQualifiedName (recommended), so this should work:
        var type = Type.GetType(typeId, throwOnError: false);
        if (type is null) return null;

        if (!typeof(Component).IsAssignableFrom(type)) return null;
        if (type.IsAbstract) return null;

        // Requires a parameterless constructor. If you have components without one, you'll need a factory.
        return Activator.CreateInstance(type) as Component;
    }
}