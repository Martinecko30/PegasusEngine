using OpenTK.Mathematics;

namespace PegasusEngine.Objects.Components;

[Serializable]
public class Camera : Component
{
    private float fov = MathHelper.PiOver2;    

    private float nearPlane = 0.01f;
    private float farPlane = 100f;
    
    public Camera() : this(16f / 9f) { }
    
    public Camera(float aspectRatio = 16f / 9f)
    {
        AspectRatio = aspectRatio;
    }
    
    public float NearPlane { get => nearPlane; set => nearPlane = value; }
    public float FarPlane { get => farPlane; set => farPlane = value; }

    public float AspectRatio;

    public Vector3 Front => Vector3.Normalize(Vector3.Transform(-Vector3.UnitZ, Transform.Rotation));
    public Vector3 Up    => Vector3.Normalize(Vector3.Transform(Vector3.UnitY, Transform.Rotation));
    public Vector3 Right => Vector3.Normalize(Vector3.Transform(Vector3.UnitX, Transform.Rotation));

    // We convert from degrees to radians as soon as the property is set to improve performance.
    public float Fov
    {
        get => MathHelper.RadiansToDegrees(fov);
        set
        {
            // Clamped to prevent math errors (FOV cannot be 0 or >= 180)
            var angle = MathHelper.Clamp(value, 1f, 179f);
            fov = MathHelper.DegreesToRadians(angle);
        }
    }

    public Matrix4 GetViewMatrix(Vector3 offset = new())
    {
        if (Transform == null) 
            return Matrix4.Identity;
        
        return Matrix4.LookAt(
            Transform.Position + offset,
            Transform.Position + Front + offset, 
            Up
        );
    }

    // Get the projection matrix using the same method we have used up until this point
    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(fov, AspectRatio, nearPlane, farPlane);
    }

    public override string ToString()
    {
        return "Camera : " + GameObject.ToString();
    }
}