using PegasusEngine.Common;
using PegasusEngine.Scripting;

namespace PegasusEngine.Objects.Components.Meshes;

[Serializable]
[RequireComponent(typeof(MeshRenderer))]
[DisallowMultipleComponents]
public class MeshFilter : Behaviour
{
    // The GUID pointing to the MeshMetadata in the AssetPool
    public GUID MeshGuid = GUID.INVALID;
}