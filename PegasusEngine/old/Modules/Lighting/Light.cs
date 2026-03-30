#region

using OpenTK.Mathematics;

#endregion

namespace PegasusEngine.old.Modules.Lighting;

public abstract class Light
{
    public Vector3 Position = Vector3.Zero;
    public Vector3 Direction = Vector3.Zero;
    public Vector3 Color = Vector3.One;
    public float Intensity { get; set; }

    // Attenuation parameters
    public float Constant { get; set; }
    public float Linear { get; set; }
    public float Quadratic { get; set; }
    
    public Light(Vector3 position)
    {
        Position = position;
    }
    
    public Light(Vector3 position, Vector3 color)
    {
        Position = position;
        Color = color;
    }
    
    public Light(Vector3 position, Vector3 color, Vector3 direction)
    {
        Position = position;
        Color = color;
        Direction = direction;
    }
}