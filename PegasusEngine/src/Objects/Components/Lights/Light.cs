using OpenTK.Mathematics;
using PegasusEngine.Export.Graphics;
using PegasusEngine.Renderer.Textures;
using PegasusEngine.Scripting;

namespace PegasusEngine.Objects.Components.Lights;

/// <summary>
/// Represents a Light component that illuminates the scene.
/// </summary>
[RequireComponent(typeof(Transform))]
public class Light : Behaviour
{
    /// <summary>
    /// Determines the type of shadows cast by this light.
    /// </summary>
    public LightShadows Shadows = LightShadows.None;
    
    /// <summary>
    /// The strength or darkness of the shadows cast by this light.
    /// A value of 1.0 represents fully dark shadows, while 0.0 means no shadows.
    /// </summary>
    public float ShadowStrength = 1.0f;
    
    /// <summary>
    /// The resolution quality of the shadows cast by this light.
    /// </summary>
    public LightShadowResolution ShadowResolution = LightShadowResolution.Medium;
    
    /// <summary>
    /// The size of the cookie mask projected by the light.
    /// </summary>
    public float CookieSize = 10.0f;

    /// <summary>
    /// The texture (cookie) projected by the light to create patterns or masks.
    /// Null if no cookie is used.
    /// </summary>
    public Texture2D Cookie { get; set; }

    /// <summary>
    /// Determines how the engine should render the light (e.g., Auto, Important, NotImportant).
    /// </summary>
    public LightRenderMode RenderMode = LightRenderMode.Auto;

    /// <summary>
    /// The physical dimensions of the light when rendering as an Area Light.
    /// </summary>
    public Vector2 AreaSize = new Vector2(1.0f, 1.0f);

    /// <summary>
    /// Determines how this light interacts with the lightmapping system (e.g., Realtime, Baked, Mixed).
    /// Primarily used in editor toolchains.
    /// </summary>
    public LightmapBakeType LightmapBakeType = LightmapBakeType.Realtime;
    
    /// <summary>
    /// Resets the light to its default parameters.
    /// </summary>
    public void Reset()
    {
        Shadows = LightShadows.None;
        ShadowStrength = 1.0f;
        ShadowResolution = LightShadowResolution.Medium;
        CookieSize = 10.0f;
        Cookie = null;
        RenderMode = LightRenderMode.Auto;
        AreaSize = new Vector2(1.0f, 1.0f);
        LightmapBakeType = LightmapBakeType.Realtime;
    }

    /// <summary>
    /// Marks the light state as dirty, forcing the renderer to update its internal buffers or uniform data next frame.
    /// </summary>
    public void SetLightDirty()
    {
        // TODO: Implement engine-specific logic to flag this component for a constant buffer/SSBO update
    }
}