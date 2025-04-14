using ImGuiNET;
using OpenTK.Mathematics;

namespace PegasusEngine.Editor.Widgets;

public class Viewport : Widget
{
    private Vector2 offset = Vector2.Zero;
    private float padding = 4.0f;
    
    private bool firstFrame = true;
    private int previousWidth = 0, previousHeight = 0;
    
    
    public Viewport(Editor editor) : base(editor)
    {
        Title = "Viewport";
        _initialSize = new(400, 250);
        _flags |= ImGuiWindowFlags.NoScrollbar;
        
    }

    public override void OnUpdateVisible()
    {
        int width = (int) Math.Round(ImGui.GetContentRegionAvail().X);
        int height = (int) Math.Round(ImGui.GetContentRegionAvail().Y);
        
        // TODO: Is resolution set?
        if (!firstFrame)
        {
            if (previousWidth != width || height != previousHeight)
            {
                
            }
        }
        firstFrame = false;
        var offset = ImGui.GetCursorPos();
        offset.Y += 34;
        // TODO: Input
        // TODO: Render framebuffer from Renderer
    }
}