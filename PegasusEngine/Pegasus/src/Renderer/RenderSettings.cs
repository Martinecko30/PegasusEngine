namespace PegasusEngine.Pegasus.Renderer;

public struct RenderSettings
{
    // Editor-only: not meant to be other than default during runtime
    public int DebugMode { get; set; } = 0; // 0 = off, 1 = aabb heatmap, 2 = triangle heatmap
    public int AabbHeatmapCutoff { get; set; } = 5000;
    public int TriangleHeatmapCutoff { get; set; } = 100;

    public uint[] Resolution { get; set; } = [400, 300];
    public int RaysPerPixel { get; set; } = 1;
    public int BouncesPerRay { get; set; } = 5;
    public bool Accumulate { get; set; } = false;
    public bool VSync { get; set; } = true;
    
    public RenderSettings()
    {
    }

    // TODO: Check Serialization/Deserialization

    public void Reset()
    {
        this = new RenderSettings();
    }
}