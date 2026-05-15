using OpenTK.Mathematics;
using PegasusEngine.Export.Graphics;

namespace PegasusEngine.Objects.Components.Lights;

/// <summary>
/// A lightweight, blittable data structure representing a light that is visible to a camera.
/// This is typically extracted from the scene by the culling system and passed to the render pipeline.
/// </summary>
public class VisibleLight : IEquatable<VisibleLight>
{
    /// <summary>
    /// The type of the light (e.g., Directional, Point, Spot).
    /// </summary>
    private LightType LightType;
    
    /// <summary>
    /// The final calculated color and intensity of the light.
    /// </summary>
    private Color4 FinalColor;
    
    /// <summary>
    /// The screen-space bounding rectangle of the light's influence.
    /// Represented as (X, Y, Width, Height). Useful for clustered/tile-based deferred rendering.
    /// </summary>
    private Vector4 ScreenRect;
    
    /// <summary>
    /// The transformation matrix representing the light's position and rotation in world space.
    /// </summary>
    private Matrix4 LocalToWorldMatrix;
    
    /// <summary>
    /// The maximum distance the light can reach. Applies primarily to Point and Spot lights.
    /// </summary>
    private float Range;
    
    /// <summary>
    /// The angle (in degrees or radians, depending on engine convention) of the light's cone. 
    /// Applies only to Spot lights.
    /// </summary>
    private float SpotAngle;
    
    /// <summary>
    /// The ID of the Entity/GameObject that owns the light component.
    /// Used to trace back to the original ECS data if needed.
    /// </summary>
    private uint EntityId;
    
    private VisibleLightFlags Flags;
    
    /// <summary>
    /// True if the light's bounding volume intersects the camera's near clipping plane.
    /// </summary>
    public bool IntersectsNearPlane
    {
        get => (Flags & VisibleLightFlags.IntersectsNearPlane) > 0;
        set
        {
            if (value)
                Flags |= VisibleLightFlags.IntersectsNearPlane;
            else
                Flags &= ~VisibleLightFlags.IntersectsNearPlane;
        }
    }
    
    /// <summary>
    /// True if the light's bounding volume intersects the camera's far clipping plane.
    /// </summary>
    public bool IntersectsFarPlane
    {
        get => (Flags & VisibleLightFlags.IntersectsFarPlane) > 0;
        set
        {
            if (value)
                Flags |= VisibleLightFlags.IntersectsFarPlane;
            else
                Flags &= ~VisibleLightFlags.IntersectsFarPlane;
        }
    }
    
    /// <summary>
    /// True if the engine forced this light to be evaluated as visible, bypassing standard frustum culling.
    /// </summary>
    public bool ForcedVisible => (Flags & VisibleLightFlags.ForcedVisible) > 0;

    /// <summary>
    /// Compares this VisibleLight to another for equality.
    /// </summary>
    public bool Equals(VisibleLight other)
    {
        return LightType == other.LightType &&
               FinalColor.Equals(other.FinalColor) &&
               ScreenRect.Equals(other.ScreenRect) &&
               LocalToWorldMatrix.Equals(other.LocalToWorldMatrix) &&
               Range.Equals(other.Range) &&
               SpotAngle.Equals(other.SpotAngle) &&
               EntityId == other.EntityId &&
               Flags == other.Flags;
    }
    
    /// <summary>
    /// Compares this VisibleLight to an object for equality.
    /// </summary>
    public override bool Equals(object obj)
    {
        return obj is VisibleLight other && Equals(other);
    }

    public static bool operator==(VisibleLight left, VisibleLight right)
    {
        return left.Equals(right);
    }

    public static bool operator!=(VisibleLight left, VisibleLight right)
    {
        return !left.Equals(right);
    }
    
    /// <summary>
    /// Generates a hash code for this VisibleLight based on its fields.
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int)LightType;
            hashCode = (hashCode * 397) ^ FinalColor.GetHashCode();
            hashCode = (hashCode * 397) ^ ScreenRect.GetHashCode();
            hashCode = (hashCode * 397) ^ LocalToWorldMatrix.GetHashCode();
            hashCode = (hashCode * 397) ^ Range.GetHashCode();
            hashCode = (hashCode * 397) ^ SpotAngle.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)EntityId;
            hashCode = (hashCode * 397) ^ (int)Flags;
            return hashCode;
        }
    }
}