using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Assimp;
using PegasusEngine.Pegasus.Core;
using StbImageSharp;

namespace PegasusEngine.Pegasus.Project.Assets;

public class AssetManager
{
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
        if (File.Exists(metaPath) && Path.HasExtension(metaPath) &&
            Path.GetExtension(metaPath) == AssetMetaFile.Extension)
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
}