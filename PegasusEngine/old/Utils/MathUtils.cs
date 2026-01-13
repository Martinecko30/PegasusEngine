#region

using OpenTK.Mathematics;

#endregion

namespace PegasusEngine.Utils;

public class MathUtils
{
    public static Vector2 TransVector2(System.Numerics.Vector2 vec)
    {
        return new Vector2(vec.X, vec.Y);
    }
    
    public static System.Numerics.Vector2 TransVector2(Vector2 vec)
    {
        return new System.Numerics.Vector2(vec.X, vec.Y);
    }
    
    public static Vector3 TransVector3(System.Numerics.Vector3 vec)
    {
        return new Vector3(vec.X, vec.Y, vec.Z);
    }
    
    public static System.Numerics.Vector3 TransVector3(Vector3 vec)
    {
        return new System.Numerics.Vector3(vec.X, vec.Y, vec.Z);
    }
}