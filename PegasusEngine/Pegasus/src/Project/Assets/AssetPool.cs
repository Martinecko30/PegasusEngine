using System.Collections.Generic;
using PegasusEngine.Pegasus.Core;
using System.Numerics;

namespace PegasusEngine.Pegasus.Project.Assets;

public record MetadataPair(Metadata Metadata, MetadataExtension Extension);

public class AssetPool
{
    public enum AssetType
    {
        Metadata,
        MeshBuffer,
        IndexBuffer,
        NodeBuffer,
        TextureBuffer,
        Count
    }

    public Dictionary<GUID, MetadataPair> Metadata { get; } = new();
    public List<Triangle> MeshBuffer { get; } = new();
    public List<uint> IndexBuffer { get; } = new();
    public List<BVHNode> NodeBuffer { get; } = new();
    public List<byte> TextureBuffer { get; } = new();

    private readonly uint[] _updateVersions = new uint[(int)AssetType.Count];

    public T? FindMetadata<T>(GUID guid) where T : Metadata
    {
        if (Metadata.TryGetValue(guid, out var pair))
            return pair.Metadata as T;
        return null;
    }
}