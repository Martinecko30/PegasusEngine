using OpenTK.Mathematics;
using PegasusEngine.Engine.Objects;

namespace PegasusEngine.Engine.Scripting;

public class Transform : Behaviour
{
    public GameObject GameObject;
    
    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public Transform(GameObject gameObject)
    {
        this.GameObject = gameObject;
    }

    public override void Update()
    {
        // Not necessary for transform
    }
}