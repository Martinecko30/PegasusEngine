using Assimp;
using OpenTK.Mathematics;
using PegasusEngine.Engine.Scripting;
using PegasusEngine.Engine.Shaders;

namespace PegasusEngine.Engine.Objects;

public class GameObject
{
    private readonly Model model;
    
    public readonly Shader Shader;
    public GameObject? Parent;
    public readonly Transform Transform;
    public readonly List<Behaviour> Behaviours = new List<Behaviour>();
    public readonly List<GameObject> Children = new List<GameObject>();

    public string Name = "default";
    
    public GameObject(string name, string modelFilePath, Vector3 position, Vector3 scale, Shader shader)
    {
        this.model = new Model(modelFilePath, scale);
        this.Transform = new Transform(this);
        this.Transform.Position = position;
        this.Transform.Scale = scale;
        this.Name = name;
        this.Shader = shader;
    }
    
    [Obsolete("Use only when overrding default shader\nDon't forget to setup all properties!")]
    public void Draw(Shader shader)
    {
        model.Draw(shader);
    }

    public void Draw()
    {
        model.Draw(this.Shader);
    }

    public virtual void Update()
    {
        foreach (Behaviour behaviour in Behaviours)
        {
            behaviour.Update();
        }
    }

    public bool CheckCollision(Vector3 point, bool grounded = true)
    {
        var boundBox = GetBoundingBox();
        
        if (grounded)
            return boundBox.ContainsInclusive(point);
        
        return point.X >= boundBox.Min.X && point.X <= boundBox.Max.X &&
               // point.Y >= boundingBox.Min.Y && point.Y <= boundingBox.Max.Y &&
               point.Z >= boundBox.Min.Z && point.Z <= boundBox.Max.Z &&
               Math.Abs(boundBox.Max.Y - point.Y) < 0.75f;
    }




    public bool CheckCollision(GameObject gameObject)
    {
        var firstBoundingBox = GetBoundingBox();
        var secondBoundingBox = gameObject.GetBoundingBox();

        var collisionX = (firstBoundingBox.Min.X >= secondBoundingBox.Min.X && 
                          firstBoundingBox.Min.X <= secondBoundingBox.Max.X) ||
                         (secondBoundingBox.Min.X >= firstBoundingBox.Min.X &&
                          secondBoundingBox.Min.X <= firstBoundingBox.Max.X);
        
        var collisionY = (firstBoundingBox.Min.Y >= secondBoundingBox.Min.Y && 
                          firstBoundingBox.Min.Y <= secondBoundingBox.Max.Y) ||
                         (secondBoundingBox.Min.Y >= firstBoundingBox.Min.Y &&
                          secondBoundingBox.Min.Y <= firstBoundingBox.Max.Y);
        
        var collisionZ = (firstBoundingBox.Min.Z >= secondBoundingBox.Min.Z && 
                          firstBoundingBox.Min.Z <= secondBoundingBox.Max.Z) ||
                         (secondBoundingBox.Min.Z >= firstBoundingBox.Min.Z &&
                          secondBoundingBox.Min.Z <= firstBoundingBox.Max.Z);
        
        return collisionX && collisionY && collisionZ;
    }
    
    public Box3 GetBoundingBox()
    {
        if (model == null)
            throw new NullReferenceException("Model is null");
        
        return model.GetBoundingBox();
    }
}