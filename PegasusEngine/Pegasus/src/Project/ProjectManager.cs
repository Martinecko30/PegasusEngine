using PegasusEngine.Pegasus.Core;
using PegasusEngine.Pegasus.Project.Assets;
using PegasusEngine.Pegasus.Project.Scenes;
using PegasusEngine.Pegasus.Renderer;

namespace PegasusEngine.Pegasus.Project;

public class ProjectManager
{
    public const string ProjectFileExtension = ".pgproj";
    private string _projectFolder = string.Empty;
    private ProjectFile _projectFile = new();
    
    public SceneManager? SceneManager { get; private set; }
    public AssetManager? AssetManager { get; private set; }
    
    public bool ProjectIsOpen => !string.IsNullOrEmpty(_projectFolder);
    public string ProjectName => Path.GetFileName(_projectFolder);
    public string ProjectFolder => _projectFolder;

    public GUID BootSceneGuid
    {
        get => _projectFile.BootSceneGuid;
        set => _projectFile.BootSceneGuid = value;
    }
    
    public bool IsBootScene(GUID guid) => _projectFile.BootSceneGuid == guid;
    public RenderSettings RuntimeRenderSettings => _projectFile.RuntimeRenderSettings;

    public bool NewProject(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            Log.EngineWarn("NewProject: Failed to create directory at {0}", folderPath);
            return false;            
        }

        try
        {
            Directory.CreateDirectory(folderPath);
            _projectFolder = folderPath;
            _projectFile = new ProjectFile();

            AssetManager = new AssetManager();
            SceneManager = new SceneManager();

            Log.EngineInfo("NewProject: Successfully created new project file at {0}", folderPath);
            return SaveProject();
        } catch (Exception e)
        {
            Log.EngineError("NewProject: Failed to create new project file at {0}", e.Message);
            return false;
        }
    }

    public bool SaveProject()
    {
        if (!ProjectIsOpen)
        {
            Log.EngineWarn("SaveProject: no project is currently open!");
            return false;
        }

        string filePath = ProjectFile.ComposeFilePath(_projectFolder);
        bool success = _projectFile.Save(filePath);
        
        SceneManager?.SaveScenesToFolder(_projectFolder);
        AssetManager?.SaveAssetPoolToFolder(_projectFolder);
        
        return success;
    }

    public bool OpenProject(string projectFilePath)
    {
        if (!File.Exists(projectFilePath) || Path.GetExtension(projectFilePath) != ProjectFile.Extension)
        {
            Log.EngineError("OpenProject: Invalid .pgproj file selected: {0}", projectFilePath);
            return false;
        }

        _projectFolder = Path.GetDirectoryName(projectFilePath) ?? string.Empty;
        var loadedFile = _projectFile.Load(projectFilePath);
        _projectFile = loadedFile ?? new ProjectFile();
        
        if (loadedFile == null)
            Log.EngineWarn("OpenProject: failed to deserialize project file at {}", projectFilePath);
        
        AssetManager = new AssetManager();
        SceneManager = new SceneManager();
        
        AssetManager.LoadAssetPoolFromFolder(_projectFolder);
        SceneManager.LoadScenesFromFolder(_projectFolder);

        if (_projectFile.BootSceneGuid != GUID.INVALID)
        {
            SceneManager.SetOpenSceneGuid(_projectFile.BootSceneGuid);
        }
        
        Log.EngineInfo("OpenProject: successfully opened project at {0}", projectFilePath);
        return true;
    }

    public void CloseProject()
    {
        if (!ProjectIsOpen)
        {
            Log.EngineWarn("CloseProject: no project is currently open!");
            return;
        }
        Log.EngineInfo("CloseProject: closing project at {0}", _projectFolder);
        
        _projectFolder = string.Empty;
        _projectFile = new ProjectFile();
        AssetManager = null;
        SceneManager = null;
        
        Log.EngineInfo("CloseProject: Project closed successfully.");
    }
}