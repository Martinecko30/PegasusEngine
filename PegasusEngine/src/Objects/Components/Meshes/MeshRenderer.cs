using System.Numerics;
using PegasusEngine.Common;
using PegasusEngine.Scripting;

namespace PegasusEngine.Objects.Components.Meshes;

[Serializable]
[RequireComponent(typeof(MeshFilter))]
[DisallowMultipleComponents]
public class MeshRenderer : Behaviour
{
    // The GUID pointing to the Material/Texture in the AssetPool
    public GUID MaterialGuid = GUID.INVALID;
    public Vector4 ColorTint = Vector4.One;
    
    public GUID DiffuseTexture = GUID.INVALID;
}