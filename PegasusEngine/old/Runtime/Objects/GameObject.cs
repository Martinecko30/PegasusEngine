#region

using OpenTK.Mathematics;
using PegasusEngine.Editor.Tabs;
using PegasusEngine.Modules.Rendering.Shaders;
using PegasusEngine.Modules.Scripting;

#endregion

namespace PegasusEngine.Runtime.Objects;

public class GameObject
{
    private readonly Model model;
    
    public readonly Shader Shader;
    public GameObject? Parent;
    public readonly Transform Transform;
    public readonly List<Behaviour> Behaviours = new List<Behaviour>();
    public readonly List<GameObject> Children = new List<GameObject>();

    public string Name = "default";

    public GameObject(GameObject gameObject)
    {
        model = gameObject.model;
        Transform = gameObject.Transform.Copy();
        Transform.Position = gameObject.Transform.Position;
        Transform.Scale = gameObject.Transform.Scale;
        Name = gameObject.Name;
        Shader = gameObject.Shader;
    }
    
    public GameObject(string name, string modelFilePath, Vector3 position, Vector3 scale, Shader shader)
    {
        model = new Model(modelFilePath, scale);
        Transform = new Transform(this);
        Transform.Position = position;
        Transform.Scale = scale;
        Name = name;
        Shader = shader;
    }
    
    [Obsolete("Use only when overrding default shader\nDon't forget to setup all properties!")]
    public void Draw(Shader shader)
    {
        model.Draw(shader);
    }

    public void Draw()
    {
        model.Draw(Shader);
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

    public GameObject Copy()
    {
        return new GameObject(this);
    }

    public T GetBehaviour<T>() where T : Behaviour
    {
        foreach (var behaviour in Behaviours)
            if (behaviour is T)
                return behaviour as T;
        
        throw new NullReferenceException("Behaviour not found!");
    }
}