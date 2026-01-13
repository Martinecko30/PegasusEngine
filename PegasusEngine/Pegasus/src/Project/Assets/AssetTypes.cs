using System.Numerics;
using System.Runtime.InteropServices;

namespace PegasusEngine.Pegasus.Project.Assets;

public abstract class Metadata;

[StructLayout(LayoutKind.Sequential)]
public struct Triangle
{
    public Vector4 V0;
    public Vector4 V1;
    public Vector4 V2;
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
    public int Width { get; set; }
    public int Height { get; set; }
    public int Channels { get; set; }
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