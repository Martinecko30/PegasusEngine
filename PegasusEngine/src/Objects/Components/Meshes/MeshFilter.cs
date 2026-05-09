using PegasusEngine.Common;

namespace PegasusEngine.Objects.Components.Meshes;

[Serializable]
[RequireComponent(typeof(MeshRenderer))]
[DisallowMultipleComponents]
public class MeshFilter : Component
{
    // The GUID pointing to the MeshMetadata in the AssetPool
    public GUID MeshGuid = GUID.INVALID;
}