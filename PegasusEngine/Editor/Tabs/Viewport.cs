using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PegasusEngine.Engine;
using Vector2 = System.Numerics.Vector2;

namespace PegasusEngine.Editor.Tabs;

public class Viewport : TabPanel
{
    public static int FBO = 0;
    private int textureID = 0;
    private int RBO = 0;
    
    public override void Start(EngineWindow engine)
    {
        base.Title = "Viewport";
        engine.Closing += OnClosing;
        CreateFramebuffer(engine.ClientSize.X, engine.ClientSize.Y);
    }

    public override void Render()
    {
        ImGui.Begin(Title);
        
        // Render viewport
        var windowSize = ImGui.GetContentRegionAvail();
        var windowWidth = (int) Math.Round(windowSize.X);
        var windowHeight = (int) Math.Round(windowSize.Y);
        RescaleFrambuffer(windowWidth, windowHeight);
        
        ImGui.Image(
            textureID,
            windowSize,
            new Vector2(0, 1),
            new Vector2(1, 0)
        );
        
        ImGui.End();
    }

    private void CreateFramebuffer(int width, int height)
    {
        FBO = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

        textureID = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureID);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear); // TODO: If doesn't work, check this
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureID, 0);
        
        RBO = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RBO);

        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
        {
            Console.WriteLine("Error creating framebuffer!");
        }
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    }
    
    private void RescaleFrambuffer(int width, int height)
    {
        GL.BindTexture(TextureTarget.Texture2D, textureID);
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width , height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear); // TODO: If doesn't work, check this
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureID, 0);

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RBO);
    }

    private void OnClosing(EventArgs args)
    {
        GL.DeleteFramebuffer(FBO);
        GL.DeleteTexture(textureID);
        GL.DeleteRenderbuffer(RBO);
    }
}