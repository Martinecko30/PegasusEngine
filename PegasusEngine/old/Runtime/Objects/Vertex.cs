#region

using OpenTK.Mathematics;

#endregion

namespace PegasusEngine.old.Runtime.Objects;

public struct Vertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoords;

    public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords)
    {
        Position = position;
        Normal = normal;
        TexCoords = texCoords;
    }
}