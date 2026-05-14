using PegasusEngine.Common;
using PegasusEngine.Core;
using PegasusEngine.Project.Scenes.Serialization;

namespace PegasusEngine.Project.Assets;

/// <summary>
/// Represents the metadata stored alongside an imported project asset.
/// </summary>
/// <remarks>
/// Asset metadata files are used to persist engine-specific information, such as the asset's
/// stable identifier and original source path, independently from the asset file itself.
/// </remarks>
[Serializable]
public record AssetMetaFile
{
    /// <summary>
    /// The file extension used for Pegasus asset metadata files.
    /// </summary>
    public const string Extension = ".pgmeta";

    /// <summary>
    /// The stable unique identifier assigned to the asset.
    /// </summary>
    /// <remarks>
    /// This identifier is used to reference the asset reliably even if the asset file is moved or renamed.
    /// </remarks>
    [SerializeField]
    public GUID Guid;
    
    /// <summary>
    /// The source path of the asset this metadata file describes.
    /// </summary>
    /// <remarks>
    /// This path identifies the asset file associated with this metadata record.
    /// </remarks>
    [SerializeField]
    public string SourcePath = string.Empty;
}