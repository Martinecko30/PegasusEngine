using PegasusEngine.Pegasus.Core;
using PegasusEngine.Pegasus.Export;
using PegasusEngine.Pegasus.Project.Scenes.Components;
using PegasusEngine.Pegasus.Renderer;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PegasusEngine.PegasusEditor;

public class EditorState
{
    private const string EditorStateFilename = "EditorState.yaml";

    public TempState Temp { get; set; } = new();

    public PersistentState Persistent { get; set; } = new();

    private static string GetFilePath() =>
        Path.Combine(EditorCfg.ResourcesPath, EditorStateFilename);

    public bool Serialize()
    {
        string filepath = GetFilePath();
        try
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(Persistent);
            File.WriteAllText(filepath, yaml);
            return true;
        }
        catch (YamlException e)
        {
            Log.EditorCritical("YAML error (invalid syntax?): {0}, error: {1}", filepath, e.Message);
            return false;
        }
        catch (Exception e)
        {
            Log.EditorCritical("Unknown error occurred while saving file: {0}, error: {1}", filepath, e.Message);
            return false;
        }
    }

    public bool Deserialize()
    {
        string filepath = GetFilePath();
        if (!File.Exists(filepath))
        {
            Log.EditorCritical("EditorState file not found: {0}", filepath);
            return false;
        }

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            string yaml = File.ReadAllText(filepath);
            Persistent = deserializer.Deserialize<PersistentState>(yaml);

            if (string.IsNullOrEmpty(Persistent.EditorThemeFilepath))
            {
                Log.EditorInfo("Using default editor theme.");
            }
            else
            {
                var (success, error) = Temp.EditorTheme.LoadFromFile(Persistent.EditorThemeFilepath);
                if (!success)
                    Log.EditorWarn("Unable to deserialize theme: {0} [Using Default Theme instead]",
                        Persistent.EditorThemeFilepath);
                else
                    Log.EditorInfo("Successfully loaded theme {0}", Persistent.EditorThemeFilepath);
            }

            return true;
        }
        catch (YamlException e)
        {
            Log.EditorCritical("YAML error (invalid syntax?): {0}, error: {1}", filepath, e.Message);
            return false;
        } catch (Exception e)
        {
            Log.EditorCritical("Unknown error occurred while loading file: {0}, error: {1}", filepath, e.Message);
            return false;
        }
    }
    

    public class TempState
    {
        public EntityHandle? SelectedEntity { get; set; } = null;
        public EditorTheme EditorTheme { get; set; } = new();
        public bool IsInRuntimeSimulation { get; set; } = false;
        
        public bool IsViewPortSettingsPanelOpen { get; set; } = false;
        public bool IsThemePanelOpen { get; set; } = false;
        public bool IsProfilerPanelOpen { get; set; } = false;
        
        public bool IsCreateProjectDialogOpen { get; set; } = false;
        public bool ShouldOpenExportPanel { get; set; } = false;
    }

    public class PersistentState
    {
        public string EditorThemeFilepath { get; set; } = string.Empty;
        public RenderSettings EditorRenderSettings { get; set; } = new();
        public ScreenFitMode ViewportMode { get; set; } = ScreenFitMode.MaxAspectFit;
    }
}