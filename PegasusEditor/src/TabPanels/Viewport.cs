using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PegasusEditor.ImGuiContext;
using PegasusEngine.Core.Events;
using PegasusEngine.Renderer;
using PegasusEngine.Renderer.Textures;
using Vector2 = System.Numerics.Vector2;

namespace PegasusEditor.TabPanels;

public class Viewport : TabPanel
{
    public static bool IS_VIEWPORT_FOCUSED = true;
    public static Vector2i WindowSize = Vector2i.One;

    private Texture2D? lastRenderedFrame;
    
    private readonly EditorState editorState;

    public Viewport(EditorState editorState)
    {
        this.editorState = editorState;
    }
    
    public override void Start()
    {
        Title = FontAwesomeIcons.Eye + " Viewport";
    }

    public override void Render()
    {
        var theme = editorState.Temp.EditorTheme;
        
        var style = ImGui.GetStyle();
        var originalWindowBG = style.Colors[(int)ImGuiCol.WindowBg];
        theme.PushColor(ImGuiCol.WindowBg, EditorCol.Background2);
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.Begin(Title);
        if (editorState.Temp.IsInRuntimeSimulation)
            ImGui.BeginDisabled();
        ImGui.PopStyleVar();

        IS_VIEWPORT_FOCUSED = ImGui.IsWindowFocused();
        
        var windowSize = ImGui.GetContentRegionAvail();
        var windowWidth = (int) Math.Round(windowSize.X);
        var windowHeight = (int) Math.Round(windowSize.Y);

        if (WindowSize.X != windowWidth || WindowSize.Y != windowHeight)
        {
            if (windowWidth > 0 && windowHeight > 0)
            {
                WindowSize = new(windowWidth, windowHeight);
                
                // TODO: Resize the framebuffer
            }
        }

        if (lastRenderedFrame != null)
        {
            ImGui.Image(
                lastRenderedFrame.textureID,
                windowSize,
                new Vector2((float)Math.Pow(windowWidth, -1), 1),
                new Vector2(1, (float)Math.Pow(windowWidth, -1))
            );
        }
        else
        {
            ImGui.Text("Waiting for engine to render...");
        }
        
        if (editorState.Temp.IsInRuntimeSimulation)
            ImGui.EndDisabled();
        
        ImGui.End();
        theme.PopColor();
    }

    public override void Update()
    {

    }

    public override void OnEvent(IEvent e)
    {
        if (e is NewFrameRenderedEvent frameRenderedEvent)
        {
            lastRenderedFrame = frameRenderedEvent.Frame;
        }
    }

    public override void Dispose()
    {
        
    }

    private void CreateFramebuffer(int width, int height)
    {
        
    }


    private void OnClosing(EventArgs args)
    {
    }
}