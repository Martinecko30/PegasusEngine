using Assimp;
using OpenTK.Mathematics;
using PegasusEngine.Engine.Shaders;
using PegasusEngine.Engine.Textures;

namespace PegasusEngine.Engine.Objects;

public class Model
{
    private List<Mesh> meshes = new List<Mesh>();
    private string directory;

    private Vector3 size = Vector3.One;
    private List<Texture> loadedTextures = new();
    
    private Box3 boundingBox;
    
    public Model(string path)
    {
        LoadModel(path);
        CalculateBoundingBox(size);
    }
    
    public Model(string path, Vector3 size)
    {
        LoadModel(path);
        this.size = size;
        CalculateBoundingBox(size);
    }

    public void Draw(Shader shader)
    {
        for (int i = 0; i < meshes.Count; i++)
            meshes[i].Draw(shader);
    }

    private void LoadModel(string path)
    {
        AssimpContext importer = new AssimpContext();
        if (!File.Exists(path))
            throw new FileNotFoundException("Could not find file: " + path + " \nWas the file set to content?");
        Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

        if (scene == null || (scene.SceneFlags & SceneFlags.Incomplete) != 0 || scene.RootNode == null)
        {
            Console.WriteLine("ERROR when loading Assimp model!");
            return;
        }

        directory = Path.GetDirectoryName(path);
        
        ProcessNode(scene.RootNode, scene);
    }

    private void CalculateBoundingBox(Vector3 scale)
    {
        boundingBox = new Box3();
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        
        foreach (Mesh mesh in meshes)
        {
            foreach(Vertex vertex in mesh.Vertices)
            {
                if(vertex.Position.X < min.X) min.X = vertex.Position.X;
                if(vertex.Position.X > max.X) max.X = vertex.Position.X;
                
                if(vertex.Position.Y < min.Y) min.Y = vertex.Position.Y;
                if(vertex.Position.Y > max.Y) max.Y = vertex.Position.Y;
                
                if(vertex.Position.Z < min.Z) min.Z = vertex.Position.Z;
                if(vertex.Position.Z > max.Z) max.Z = vertex.Position.Z;
            }
        }
        
        boundingBox.Min = min * scale;
        boundingBox.Max = max * scale;
    }

    private void ProcessNode(Node node, Scene scene)
    {
        for (int i = 0; i < node.MeshCount; i++)
        {
            Assimp.Mesh mesh = scene.Meshes[node.MeshIndices[i]];
            meshes.Add(ProcessMesh(mesh, scene));
        }

        for (int i = 0; i < node.ChildCount; i++)
        {
            ProcessNode(node.Children[i], scene);
        }
    }

    private Mesh ProcessMesh(Assimp.Mesh mesh, Scene scene)
    {
        Vertex[] vertices = new Vertex[mesh.VertexCount];
        List<uint> indices = new List<uint>();
        List<Texture> textures = new List<Texture>();

        
        // Process vertices, normals, texture coordinates
        for (int i = 0; i < mesh.VertexCount; i++)
        {
            Vertex vertex = new Vertex();
            Vector3D position = mesh.Vertices[i];
            vertex.Position = new Vector3(position.X, position.Y, position.Z);
            
            if (mesh.HasNormals)
            {
                Vector3D normal = mesh.Normals[i];
                vertex.Normal = new Vector3(normal.X, normal.Y, normal.Z);
            }

            if (mesh.TextureCoordinateChannelCount > 0 && mesh.HasTextureCoords(0))
            {
                Vector3D texCoord = mesh.TextureCoordinateChannels[0][i];
                vertex.TexCoords = new Vector2(texCoord.X, texCoord.Y);
            }

            vertices[i] = vertex;
        }
        
        // Process indices
        for (int i = 0; i < mesh.FaceCount; i++)
        {
            Face face = mesh.Faces[i];
            foreach (var index in face.Indices)
                indices.Add((uint) index);
        }

        // Process material
        if (mesh.MaterialIndex >= 0)
        {
            Material material = scene.Materials[mesh.MaterialIndex];
            List<Texture> diffuseMaps = LoadMaterialTextures(
                material, 
                TextureType.Diffuse, 
                "diffuseTexture"
                );
            textures.AddRange(diffuseMaps);
            
            List<Texture> specularMaps = LoadMaterialTextures(
                material, 
                TextureType.Specular, 
                "specularTexture"
            );
            textures.AddRange(specularMaps);

            List<Texture> normalMaps = LoadMaterialTextures(
                material,
                TextureType.Height,
                "normalTexture"
            );
            textures.AddRange(normalMaps);
            
            List<Texture> heightMaps = LoadMaterialTextures(
                material,
                TextureType.Ambient,
                "heightTexture"
            );
            textures.AddRange(heightMaps);
        }

        return new Mesh(vertices, indices, textures);
    }

    private List<Texture> LoadMaterialTextures(Material mat, TextureType type, string typeName)
    {
        List<Texture> textures = new List<Texture>();
        
        //Console.WriteLine(mat.GetMaterialTextureCount(type) + " " + type + " " + mat.Name + " " + directory);
        

        if (type == TextureType.Diffuse && mat.GetMaterialTextureCount(type) <= 0 && mat.HasColorDiffuse)
        {
            Texture texture = new Texture(typeName, mat.ColorDiffuse);
            textures.Add(texture);
            loadedTextures.Add(texture);
            return textures;
        }
        
        if (type == TextureType.Specular && mat.HasColorSpecular && mat.GetMaterialTextureCount(type) <= 0)
        {
            Texture texture = new Texture(typeName, mat.ColorSpecular);
            textures.Add(texture);
            loadedTextures.Add(texture);
            return textures;
        }
        
        for (int i = 0; i < mat.GetMaterialTextureCount(type); i++)
        {
            mat.GetMaterialTexture(type, i, out TextureSlot textureSlot);
            string str = textureSlot.FilePath;
            
            bool skip = false;
            foreach (var texture in loadedTextures)
            {
                if (texture.path.Equals(str))
                {
                    textures.Add(texture);
                    skip = true;
                    break;
                }
            }

            if (!skip)
            {
                Texture texture = new Texture(Path.Combine(directory, textureSlot.FilePath), typeName);
                textures.Add(texture);
                loadedTextures.Add(texture);
            }
        }

        return textures;
    }

    public Box3 GetBoundingBox()
    {
        if(boundingBox == null)
            CalculateBoundingBox(size);
        return boundingBox;
    }
}