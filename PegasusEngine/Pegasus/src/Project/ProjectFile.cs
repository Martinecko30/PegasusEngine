using PegasusEngine.Pegasus.Core;
using PegasusEngine.Pegasus.Renderer;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PegasusEngine.Pegasus.Project;

public struct ProjectFile
{
    public const string Extension = ".pgproj";
    
    public GUID BootSceneGuid { get; set; } = GUID.INVALID;
    public RenderSettings RuntimeRenderSettings { get; set; } = new();
    
    public ProjectFile() {}
    
    public ProjectFile(GUID bootSceneGuid)
    {
        BootSceneGuid = bootSceneGuid;
    }

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

    public ProjectFile? Load(string filePath)
    {
        if (!File.Exists(filePath) || Path.GetExtension(filePath) != Extension) 
            return null;

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
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