using OpenTK.Mathematics;

namespace PegasusEditor.Renderer;

[Obsolete("Use Camera instead")]
public class EditorCamera
{
    public Vector3 Position { get; set; } = new Vector3(0f, 2f, 5f);
    
    // Euler Angles (in radians)
    public float Pitch { get; set; } = 0f; 
    public float Yaw { get; set; } = -MathHelper.PiOver2; // Pointing down negative Z
    
    public float MoveSpeed { get; set; } = 5.0f;
    public float MouseSensitivity { get; set; } = 0.003f;
    
    public float Fov { get; set; } = MathHelper.PiOver3; // 60 degrees
    public float AspectRatio { get; set; } = 16f / 9f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000f;

    // Calculated Direction Vectors
    public Vector3 Front => new Vector3(
        MathF.Cos(Pitch) * MathF.Cos(Yaw),
        MathF.Sin(Pitch),
        MathF.Cos(Pitch) * MathF.Sin(Yaw)
    ).Normalized();

    public Vector3 Right => Vector3.Cross(Front, Vector3.UnitY).Normalized();
    public Vector3 Up => Vector3.Cross(Right, Front).Normalized();

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + Front, Up);
    }

    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(Fov, AspectRatio, NearPlane, FarPlane);
    }

    public void ProcessMouse(Vector2 mouseDelta)
    {
        Yaw += mouseDelta.X * MouseSensitivity;
        Pitch -= mouseDelta.Y * MouseSensitivity; // ImGui Y is inverted

        float limit = MathHelper.PiOver2 - 0.01f;
        Pitch = MathHelper.Clamp(Pitch, -limit, limit);
    }

    public void ProcessKeyboard(Vector3 direction, float deltaTime)
    {
        Position += direction * MoveSpeed * deltaTime;
    }
}