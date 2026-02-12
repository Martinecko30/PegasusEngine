namespace PegasusEngine.Pegasus.Export;

public enum ScreenFitMode
{
    OriginalCentered,
    StretchFill,
    MaxAspectFit
}

public class ExportSettings
{
    public bool Fullscreen { get; set; } = false;
    public bool VSync { get; set; } = true;
    public ScreenFitMode ScreenFitMode { get; set; } = ScreenFitMode.MaxAspectFit;
}