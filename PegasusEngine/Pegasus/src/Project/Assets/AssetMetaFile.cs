using PegasusEngine.Pegasus.Core;
using YamlDotNet.Core;

namespace PegasusEngine.Pegasus.Project.Assets;

public record AssetMetaFile
{
    public const string Extension = ".pgmeta";
    
    public GUID Guid {get; set;}
    public string SourcePath {get; set;} = string.Empty;
}