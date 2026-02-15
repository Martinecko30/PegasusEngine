using System.Diagnostics;
using System.Numerics;
using Assimp;
using PegasusEngine.Core;
using StbImageSharp;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PegasusEngine.Project.Assets;

public class AssetManager
{
    public static readonly string[] SupportedMeshFileFormats = [".fbx", ".obj"];
    public static readonly string[] SupportedTextureFileFormats = [".png", ".jpg", ".jpeg", ".tga", ".bmp", ".hdr"];
    
    private readonly AssetPool _assetPool = new();
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;
    
    public AssetPool AssetPool => _assetPool;

    public AssetManager()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
    }
    
    // ================================================
    // META FILE HANDLING
    // ================================================

    public bool SaveMetaFile(string metaPath, AssetMetaFile metaFile)
    {
        if (!(Path.HasExtension(metaPath) && Path.GetExtension(metaPath).ToLower() == AssetMetaFile.Extension))
        {
            Log.EngineError("SaveMetaFile: invalid file extension {0}.", metaPath);
            return false;
        }

        try
        {
            string yaml = _serializer.Serialize(metaFile);
            File.WriteAllText(metaPath, yaml);
            Log.EngineInfo("SaveMetaFile: wrote metadata for GUID {0}.", metaFile.Guid);
        } catch (Exception e)
        {
            Log.EngineError("SaveMetaFile: failed to write metadata for GUID {0}.", metaFile.Guid);
            Log.EngineError(e.ToString());
            return false;
        }
        
        return true;
    }

    public AssetMetaFile? LoadMetaFile(string metaPath)
    {
        if (!File.Exists(metaPath) || !Path.HasExtension(metaPath) ||
            Path.GetExtension(metaPath) != AssetMetaFile.Extension)
        {
            Log.EngineWarn("LoadMetaFile: invalid or missing meta file: {0}", metaPath);
            return null;
        }

        try
        {
            string yaml = File.ReadAllText(metaPath);
            var meta = _deserializer.Deserialize<AssetMetaFile>(yaml);
            Log.EngineInfo("LoadMetaFile: loaded metadata for GUID {0}.", meta.Guid);
            return meta;
        }
        catch (Exception e)
        {
            Log.EngineError("LoadMetaFile: failed to load {0}", metaPath);
            Log.EngineError(e.ToString());
        }
        return null;
    }
    
    
    
    public GUID ImportAsset(string assetPath)
    {
        if (!File.Exists(assetPath))
        {
            Log.EngineWarn("ImportAsset: invalid asset path: {0}", assetPath);
            return GUID.INVALID;
        }

        var guid = new GUID();
        if (!LoadAssetFile(assetPath, guid))
        {
            Log.EngineWarn("ImportAsset: failed to load asset after saving metafile, removed metafile: {0}", assetPath);
            return GUID.INVALID;
        }
        
        Log.EngineInfo("ImportAsset: imported asset {0} with GUID {1}", assetPath, guid);
        return guid;
    }

    public void SaveAssetPoolToFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            foreach (var metaPath in Directory.GetFiles(folderPath, "*" + AssetMetaFile.Extension))
            {
                var metaFile = LoadMetaFile(metaPath);
                if (metaFile == null)
                {
                    Log.EngineWarn("SaveAssetPoolToFolder: unable to read metafile {0}", metaPath);
                    continue;
                }

                GUID guid = metaFile.Guid;
                if (!AssetPool.Metadata.ContainsKey(guid))
                {
                    try
                    {
                        File.Delete(metaPath);
                        Log.EngineInfo("SaveAssetPoolToFolder: removed stale metafile {0}", metaPath);
                    }
                    catch (Exception e)
                    {
                        Log.EngineError("SaveAssetPoolToFolder: failed to remove stale metafile {0}. {1}",
                            metaPath, e.ToString());
                    }
                }
            }
        }

        foreach (var (guid, metadataPair) in AssetPool.Metadata)
        {
            var extension = metadataPair.Extension;
            if (File.Exists(extension.SourcePath))
            {
                var metaFile = new AssetMetaFile
                {
                    Guid = guid,
                    SourcePath = extension.SourcePath
                };

                string fileName = Path.GetFileName(extension.SourcePath);
                string metaPath = Path.Combine(folderPath, fileName + AssetMetaFile.Extension);

                if (!SaveMetaFile(metaPath, metaFile))
                    Log.EngineWarn("SaveAssetPoolToFolder: failed to save metafile {0}", metaPath);
                else
                    Log.EngineInfo("SaveAssetPoolToFolder: saved metafile {0}", metaPath);
            }
            else
            {
                Log.EngineWarn("SaveAssetPoolToFolder: asset does not exist {0}", extension?.SourcePath ?? "null");
            }
        }
    }

    public void LoadAssetPoolFromFolder(string folderPath)
    {
        foreach (var metaPath in Directory.GetFiles(folderPath, "*" + AssetMetaFile.Extension))
        {
            var metaFile = LoadMetaFile(metaPath);
            if (metaFile == null)
            {
                Log.EngineWarn("LoadAssetPoolFromFolder: failed to load metafile {0}", metaPath);
                continue;
            }
            
            var sourcePath = metaFile.SourcePath;
            if (!Path.Exists(sourcePath))
            {
                Log.EngineWarn("LoadAssetPoolFromFolder: missing asset file for metafile {0}", metaPath);
                continue;
            }

            if (!LoadAssetFile(sourcePath, metaFile.Guid))
            {
                Log.EngineWarn("LoadAssetPoolFromFolder: failed to load asset {0}", sourcePath);
                continue;
            }
            
            Log.EngineInfo("LoadAssetPoolFromFolder: loaded asset {0} with GUID {1}", sourcePath, metaFile.Guid);
        }
    }

    public bool LoadAssetFile(string assetPath, GUID guid)
    {
        if (!File.Exists(assetPath) || !Path.HasExtension(assetPath))
        {
            Log.EngineWarn("LoadAssetFile: invalid asset path: {0}", assetPath);
            return false;
        }
        
        var extension = Path.GetExtension(assetPath);
        if (SupportedMeshFileFormats.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            Log.EngineInfo("LoadAssetFile: loading mesh {0} for GUID {1}", assetPath, guid);
            return LoadMesh(assetPath, guid);
        }
        if (SupportedTextureFileFormats.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            Log.EngineInfo("LoadAssetFile: loading texture {0} for GUID {1}", assetPath, guid);
            return LoadTexture(assetPath, guid, 4);
        }
        
        Log.EngineWarn("LoadAssetFile: unsupported file extension {0}", extension);
        return false;
    }

    public bool LoadMesh(string assetPath, GUID guid)
    {
        var timerStart = Stopwatch.GetTimestamp();
        
        using AssimpContext importer = new AssimpContext();
        var scene = importer.ImportFile(Path.GetFullPath(assetPath));
        if (scene == null || (scene.SceneFlags & SceneFlags.Incomplete) != 0 || scene.RootNode == null)
        {
            Log.EngineCritical("LoadMesh: failed to load assimp scene from {0} (GUID {1})", assetPath, guid);
            return false;
        }

        var triCount = 0;
        scene.Meshes.ForEach(mesh => triCount += mesh.FaceCount);
        var meshBuffer = AssetPool.MeshBuffer;

        var metadata = new MeshMetadata();
        metadata.FirstTriIdx = (uint)meshBuffer.Count;
        metadata.TriCount = (uint)triCount;

        var metadataExtension = new MeshMetadataExtension();
        metadataExtension.SourcePath = assetPath;
        metadataExtension.FileSizeInBytes = (uint) new FileInfo(assetPath).Length;

        meshBuffer.Capacity += triCount;

        for (int i = 0; i < scene.MeshCount; i++)
        {
            var subMesh = scene.Meshes[i];
            var verts = subMesh.Vertices;

            for (int j = 0; j < subMesh.FaceCount; j++)
            {
                var face = subMesh.Faces[j];
                if (face.IndexCount != 3)
                    continue;

                if (!face.HasIndices)
                {
                    Log.EngineError("LoadMesh: mesh {0} has invalid face with no indices", assetPath);
                    continue;
                }
                
                var idx = face.Indices;
                meshBuffer.Add(new Triangle
                {
                    V0 = new Vector4(verts[idx[0]].X, verts[idx[0]].Y, verts[idx[0]].Z, .0f),
                    V1 = new Vector4(verts[idx[1]].X, verts[idx[1]].Y, verts[idx[1]].Z, .0f),
                    V2 = new Vector4(verts[idx[2]].X, verts[idx[2]].Y, verts[idx[2]].Z, .0f)
                });
            }
        }
        
        AssetPool.MarkUpdated(AssetPool.AssetType.MeshBuffer);
        
        BVHAccel bvh = new(meshBuffer, metadata.FirstTriIdx, metadata.TriCount);
        bvh.Build(AssetPool.NodeBuffer, AssetPool.IndexBuffer, out uint firstNodeIdx, out uint nodeCount);
        metadata.FirstTriIdx = firstNodeIdx;
        metadata.NodeCount = nodeCount;
        
        AssetPool.MarkUpdated(AssetPool.AssetType.NodeBuffer);
        AssetPool.MarkUpdated(AssetPool.AssetType.IndexBuffer);
        
        var endTimer = Stopwatch.GetTimestamp();
        double loadTimeMs = (double)(endTimer - timerStart) * 1000 / Stopwatch.Frequency;
        metadataExtension.LoadTimeMs = (uint)loadTimeMs;

        AssetPool.Metadata[guid] = new MetadataPair(metadata, metadataExtension);
        AssetPool.MarkUpdated(AssetPool.AssetType.Metadata);
        Log.EngineInfo("LoadMesh: loaded {0} triangles from {1} (GUID {2}) inf {3:.2f} ms",
            triCount, assetPath, guid, loadTimeMs);
        return true;
    }

    public bool LoadTexture(string assetPath, GUID guid, int channels)
    {
        var timerStart = Stopwatch.GetTimestamp();

        StbImage.stbi_set_flip_vertically_on_load(1);
        
        ColorComponents requiredComponents = (ColorComponents)channels;
        ImageResult? image;
        using (Stream stream = File.OpenRead(assetPath))
        {
            image = ImageResult.FromStream(stream, requiredComponents);
        }

        if (image is null)
        {
            Log.EngineCritical("LoadTexture: failed to load texture from path={0} (requested channels={1]) for GUID={2}",
                assetPath, channels, guid);
            return false;
        }

        byte[] data = image.Data;
        int totalBytes = data.Length;

        var textureBuffer = AssetPool.TextureBuffer;
        var texStartIdx = textureBuffer.Count;
        textureBuffer.AddRange(data);

        var metadata = new TextureMetadata
        {
            TexStartIdx = (uint)texStartIdx,
            Width = (uint)image.Width,
            Height = (uint)image.Height,
            Channels = (uint)image.SourceComp
        };

        var metadataExtension = new TextureMetadataExtension
        {
            SourcePath = assetPath,
            FileSizeInBytes = (uint)new FileInfo(assetPath).Length
        };

        var endTimer = Stopwatch.GetTimestamp();
        double loadTimeMs = (double)(endTimer - timerStart) * 1000 / Stopwatch.Frequency;
        metadataExtension.LoadTimeMs = (uint)loadTimeMs;
        
        AssetPool.Metadata[guid] = new MetadataPair(metadata, metadataExtension);
        AssetPool.MarkUpdated(AssetPool.AssetType.TextureBuffer);
        AssetPool.MarkUpdated(AssetPool.AssetType.Metadata);
        
        Log.EngineInfo("LoadTexture: loaded texture {0} (GUID {1} {2}x{3} with {4} channels in {5:F2}ms",
            assetPath, guid, image.Width, image.Height, image.SourceComp, loadTimeMs);
        return true;
    }
}