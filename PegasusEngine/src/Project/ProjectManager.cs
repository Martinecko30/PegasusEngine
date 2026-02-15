using System.Diagnostics;
using PegasusEngine.Core;
using PegasusEngine.Project.Assets;
using PegasusEngine.Project.Scenes;
using PegasusEngine.Renderer;

namespace PegasusEngine.Project;

public sealed class ProjectManager
{
    public const string ProjectFileExtension = ".pgproj";
    private string _projectFolder = string.Empty;
    private string _csProjectPath = string.Empty;
    private ProjectFile _projectFile = new();
    
    
    public SceneManager? SceneManager { get; private set; }
    public AssetManager? AssetManager { get; private set; }
    
    public bool ProjectIsOpen => !string.IsNullOrEmpty(_projectFolder);
    public string ProjectName => Path.GetFileName(_projectFolder);
    public string ProjectFolder => _projectFolder;
    public string CSProjectPath => _csProjectPath;
    public string AbsoluteCSProjectPath 
    {
        get
        {
            if (string.IsNullOrEmpty(CSProjectPath)) return string.Empty;
            return Path.Combine(_projectFolder, CSProjectPath);
        }
    }

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

        if (!UpdateCSProjectPath())
            CreateDefaultCSProject();

        string filePath = ProjectFile.ComposeFilePath(_projectFolder);
        bool success = _projectFile.Save(filePath);
        
        SceneManager?.SaveScenesToFolder(_projectFolder);
        AssetManager?.SaveAssetPoolToFolder(_projectFolder);
        
        return success;
    }
    
    /// <summary>
    /// Searches the current project folder for a .csproj file and updates the project data.
    /// </summary>
    private bool UpdateCSProjectPath()
    {
        if (!ProjectIsOpen)
            return true;

        string[] csprojFiles = Directory.GetFiles(_projectFolder, "*.csproj", SearchOption.AllDirectories);

        if (csprojFiles.Length > 0)
        {
            string relativePath = Path.GetRelativePath(_projectFolder, csprojFiles[0]);
            
            _csProjectPath = relativePath;
            
            Log.EngineInfo("UpdateCSProjectPath: Script project found at {0}", relativePath);
            return true;
        }
        else
        {
            Log.EngineWarn("UpdateCSProjectPath: No .csproj found in {0}", _projectFolder);
            _csProjectPath = string.Empty;
            return false;
        }
    }

    private bool CreateDefaultCSProject()
    {
        try
        {
            string fullPath = Path.Combine(Path.GetFullPath(_projectFolder), $"{ProjectName}.csproj");
            string engineDllPath = Path.Combine(AppContext.BaseDirectory, "PegasusEngine.dll");

            string csprojContent = 
$@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include=""PegasusEngine"">
      <HintPath>{engineDllPath}</HintPath>
      <Private>False</Private>
    </Reference>
  </ItemGroup>
</Project>";
            
            if (!File.Exists(fullPath))
                File.Create(fullPath).Close();
            
            File.WriteAllText(fullPath, csprojContent);
            _csProjectPath = Path.GetRelativePath(_projectFolder, fullPath);
            
            Log.EngineInfo("CreateDefaultCSProject: Created default C# project at {0}", fullPath);
            return true;
        }
        catch (Exception e)
        {
            Log.EngineError("CreateDefaultCSProject: Failed to create CSProject. {0}", e.Message);
            return false;
        }
    }

    public bool OpenProject(string projectFilePath)
    {
        if (!File.Exists(projectFilePath) || Path.GetExtension(projectFilePath) != ProjectFile.Extension)
        {
            Log.EngineError("OpenProject: Invalid .pgproj file selected: {0}", projectFilePath);
            return false;
        }

        _projectFolder = Path.GetDirectoryName(projectFilePath) ?? string.Empty;
        var loadedFile = ProjectFile.Load(projectFilePath);
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

        UpdateCSProjectPath();
        
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