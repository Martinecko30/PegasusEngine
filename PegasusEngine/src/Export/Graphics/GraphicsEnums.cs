namespace PegasusEngine.Export.Graphics;

public enum LightShadows
{
    None = 0,
    Hard = 1,
    Soft = 2
}

public enum FogMode
{
    Linear = 1,
    Exponential = 2,
    ExponentialSquared = 3
}

[Flags]
public enum LightmapBakeType
{
    Realtime = 4,
    Baked = 2,
    Mixed = 1
}

public enum LightShadowResolution
{
    FromQualitySettings = -1,
    Low = 0,
    Medium = 1,
    High = 2,
    VeryHigh = 3
}


public enum LightRenderMode
{
    Auto = 0,
    ForcePixel = 1,
    ForceVertex = 2
}

public enum LightType
{
    Spot = 0,
    Directional = 1,
    Point = 2,
    Rectangle = 3,
    Disc = 4,
    Pyramid = 5,
    Box = 6,
    Tube = 7
}

[Flags]
enum VisibleLightFlags
{
    IntersectsNearPlane = 1 << 0,
    IntersectsFarPlane = 1 << 1,
    ForcedVisible = 1 << 2,
}