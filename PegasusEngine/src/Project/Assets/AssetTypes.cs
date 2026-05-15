using System.Numerics;
using System.Runtime.InteropServices;

namespace PegasusEngine.Project.Assets;

public abstract class Metadata;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct Vertex
{
    public Vector4 Position; // X, Y, Z, 1.0
    public Vector4 Normal;   // Nx, Ny, Nz, 0.0
    public Vector4 TexCoord; // U, V, 0.0, 0.0
    public Vector4 Color;
}

[StructLayout(LayoutKind.Sequential, Pack = 16)] // Pack=16 ensures std140/std430 alignment
public struct Triangle
{
    public Vertex V0;
    public Vertex V1;
    public Vertex V2;
}

public class MeshMetadata : Metadata
{
    public uint FirstTriIdx { get; set; }
    public uint TriCount { get; set; }
    public uint FirstNodeIdx { get; set; }
    public uint NodeCount { get; set; }
}

public class TextureMetadata : Metadata
{
    public uint TexStartIdx { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }
    public uint Channels { get; set; }
}

// Extensions with additional metadata of assets
// Renderer is not fed these
public abstract class MetadataExtension
{
    public float LoadTimeMs { get; set; } = -1;
    public string SourcePath { get; set; } = string.Empty;
    public ulong FileSizeInBytes { get; set; } = 0;
}

public class MeshMetadataExtension : MetadataExtension
{
    /* additional mesh specific fields ... */
}

public class TextureMetadataExtension : MetadataExtension
{
    /* additional texture specific fields ... */
}