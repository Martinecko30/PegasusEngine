using System.Numerics;
using PegasusEngine.Common;

namespace PegasusEngine.Objects.Components.Meshes;

[Serializable]
[RequireComponent(typeof(MeshFilter))]
public class MeshRenderer : Component
{
    // The GUID pointing to the Material/Texture in the AssetPool
    public GUID MaterialGuid = GUID.INVALID;
    public Vector4 ColorTint = Vector4.One;
}