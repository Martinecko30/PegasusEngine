using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PegasusEditor.ImGuiContext;
using PegasusEngine.Common;
using PegasusEngine.Core.Events;
using PegasusEngine.Core.Layers;
using PegasusEngine.Objects;
using PegasusEngine.Objects.Components;
using PegasusEngine.Renderer.Textures;
using Log = PegasusEngine.Debug.Log;
using Vector2 = System.Numerics.Vector2;

namespace PegasusEditor.TabPanels;

public class Viewport : TabPanel
{
    private bool isViewportFocused = false;
    private bool isViewportHovered = false;

    private bool isFlying = false;
    
    public static Vector2i WindowSize = Vector2i.One;

    private Texture2D? lastRenderedFrame;
    
    private readonly EditorState editorState;
    private readonly IEventDispatcher eventDispatcher;
    private readonly GameWindow window;

    private GameObject cameraGO;
    private Camera camera = new();
    private float cameraPitch;
    private float cameraYaw;
    

    public Viewport(EditorState editorState, IEventDispatcher eventDispatcher, GameWindow window)
    {
        this.eventDispatcher = eventDispatcher;
        this.editorState = editorState;
        this.window = window;
    }
    
    public override void Start()
    {
        Title = FontAwesomeIcons.Eye + " Viewport";
        
        cameraGO = new GameObject(new GUID(), "ViewPort Camera");
        camera = cameraGO.AddComponent<Camera>();
        
        camera.Transform.Position = new Vector3(0, 5.0f, 5.0f);
        
        cameraPitch = MathHelper.DegreesToRadians(-45f); 
        camera.Transform.Rotation = Quaternion.FromEulerAngles(cameraPitch, 0, 0);
        
        eventDispatcher.DispatchEvent(new RenderCameraChangedEvent(camera));
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

        isViewportFocused = ImGui.IsWindowFocused();
        isViewportHovered = ImGui.IsWindowHovered();
        
        var windowSize = ImGui.GetContentRegionAvail();
        var windowWidth = (int) Math.Round(windowSize.X);
        var windowHeight = (int) Math.Round(windowSize.Y);

        if (WindowSize.X != windowWidth || WindowSize.Y != windowHeight)
        {
            if (windowWidth > 0 && windowHeight > 0)
            {
                WindowSize = new(windowWidth, windowHeight);
                
                camera.AspectRatio = (float) windowWidth / windowHeight;
                
                // TODO: Resize the framebuffer
            }
        }
        
        if (lastRenderedFrame != null)
        {
            ImGui.Image(
                lastRenderedFrame.textureID, 
                windowSize,
                new Vector2(0, 1),
                new Vector2(1, 0)
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
        if (isViewportHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            isFlying = true;

            window.CursorState = CursorState.Grabbed;
        }

        if (isFlying && !ImGui.IsMouseDown(ImGuiMouseButton.Right))
        {
            isFlying = false;
            
            window.CursorState = CursorState.Normal;
        }
        
        if (isFlying)
        {
            var io = ImGui.GetIO();

            const float MouseSensitivity = 0.003f;
            cameraYaw -= io.MouseDelta.X * MouseSensitivity;
            cameraPitch -= io.MouseDelta.Y * MouseSensitivity;

            float limit = MathHelper.PiOver2 - 0.01f;
            cameraPitch = MathHelper.Clamp(cameraPitch, -limit, limit);
            
            Quaternion qYaw = Quaternion.FromAxisAngle(Vector3.UnitY, cameraYaw);
            Quaternion qPitch = Quaternion.FromAxisAngle(Vector3.UnitX, cameraPitch);
            camera.Transform.Rotation = qYaw * qPitch;



            Vector3 movement = Vector3.Zero;
            if (ImGui.IsKeyDown(ImGuiKey.W)) movement += camera.Front;
            if (ImGui.IsKeyDown(ImGuiKey.S)) movement -= camera.Front;
            if (ImGui.IsKeyDown(ImGuiKey.A)) movement -= camera.Right;
            if (ImGui.IsKeyDown(ImGuiKey.D)) movement += camera.Right;
            
            // Q/E for Up/Down
            if (ImGui.IsKeyDown(ImGuiKey.E)) movement += Vector3.UnitY;
            if (ImGui.IsKeyDown(ImGuiKey.Q)) movement -= Vector3.UnitY;

            if (movement.LengthSquared > 0)
                movement.Normalize();
            
            float speedMult = ImGui.IsKeyDown(ImGuiKey.ModShift) ? 2.5f : 1.0f;
            
            const float MoveSpeed = 5.0f;
            camera.Transform.Position += (movement * speedMult) * MoveSpeed * Time.DeltaTime;
        }
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

    private void OnClosing(EventArgs args)
    {
    }
}