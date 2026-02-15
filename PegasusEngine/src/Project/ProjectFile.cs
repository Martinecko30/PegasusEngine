using PegasusEngine.Core;
using PegasusEngine.Project.Scenes.Serialization;
using PegasusEngine.Renderer;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PegasusEngine.Project;

public struct ProjectFile
{
    public const string Extension = ".pgproj";
    
    public GUID BootSceneGuid { get; set; } = GUID.INVALID;
    public RenderSettings RuntimeRenderSettings { get; set; } = new();

    public ProjectFile()
    {
    }
    
    public ProjectFile(GUID bootSceneGuid) : this()
    {
        BootSceneGuid = bootSceneGuid;
    }


    /// <summary>
    /// Saves current Project file.
    /// </summary>
    /// <param name="filePath">The file into which the save will be written</param>
    /// <returns>
    /// true - if saving was successful
    /// false - otherwise
    /// </returns>
    public bool Save(string filePath)
    {
        if (Path.GetExtension(filePath) != Extension)
        {
            Log.EngineWarn("SaveProjectFile: invalid file extension '{0}'.", filePath);
            return false;
        }

        try
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .WithTypeConverter(new GUIDYamlConverter())
                .Build();

            var yaml = serializer.Serialize(this);
            File.WriteAllText(filePath, yaml);
            Log.EngineInfo("SaveProjectFile: wrote project data into {0}", filePath);
            return true;
        }
        catch (Exception e)
        {
            Log.EngineError("SaveProjectFile: failed to save {0}. {1}", filePath, e.Message);
            return false;
        }
    }

    public static ProjectFile? Load(string filePath)
    {
        if (!File.Exists(filePath) || Path.GetExtension(filePath) != Extension) 
            return null;

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .WithTypeConverter(new GUIDYamlConverter())
                .Build();

            var yaml = File.ReadAllText(filePath);
            var result = deserializer.Deserialize<ProjectFile>(yaml);
            Log.EngineInfo("LoadProjectFile: successfully loaded project file from {0}", filePath);
            return result;
        }
        catch (Exception e)
        {
            Log.EngineError("LoadProjectFile: failed to load {0}. {1}", filePath, e.Message);
            return null;
        }
    }
    
    public static string ComposeFilePath(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
            return string.Empty;
        var folderName = Path.GetFileName(folderPath);
        return Path.Combine(folderPath, folderName + Extension);
    }
}