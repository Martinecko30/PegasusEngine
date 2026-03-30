using PegasusEngine.Common;
using PegasusEngine.Core;

namespace PegasusEngine.Project.Assets;

public record AssetMetaFile
{
    public const string Extension = ".pgmeta";
    
    public GUID Guid {get; set;}
    public string SourcePath {get; set;} = string.Empty;
}