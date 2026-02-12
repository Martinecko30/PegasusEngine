using System.Collections;
using PegasusEngine.Pegasus.Core;

namespace PegasusEngine.Pegasus.Project.Scenes;

public class SceneManager : IEnumerable<KeyValuePair<GUID, Scene>>
{
    public const string SceneFileExtension = ".pgscene";

    private GUID _openSceneGuid = GUID.INVALID;
    private GUID _openSceneGuidCache = GUID.INVALID;
    private bool _inRuntimeSimulation = false;

    private readonly Dictionary<GUID, Scene> _scenes = new();
    private readonly Dictionary<GUID, Scene> _runtimeSimulationScenes = new();

    // Replaces the C++ raw pointer swap logic
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

        // 1. Delete orphaned .lrscene files
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
    private bool SaveSceneFile(string path, Scene scene) => throw new NotImplementedException();
    private Scene? LoadSceneFile(string path) => throw new NotImplementedException();
}